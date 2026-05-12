using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using Telecom_ThesisProject.Core;
using Telecom_ThesisProject.MVVM.Model;
using Telecom_ThesisProject.Services;
using Telecom_ThesisProject.Utilities.DTOs;
using Telecom_ThesisProject.Utilities.Interfaces;

namespace Telecom_ThesisProject.MVVM.ViewModel;

class NetworkDeviceAddEditViewModel : ObservableObject
{
    private readonly NetworkDeviceService _networkDeviceService;
    private readonly INetworkDevicePoller _poller;
    private readonly IMessageService _messageService;

    #region Properties

    public NetworkDevice Device { get; }

    private NetworkDevice? _selectedParent;
    public NetworkDevice? SelectedParent
    {
        get => _selectedParent;
        set
        {
            _selectedParent = value;
            Device.ParentDeviceId = value?.Id;
            OnPropertyChanged();
        }
    }

    public SnmpProfile? _selectedSnmpProfile;
    public SnmpProfile? SelectedSnmpProfile
    {
        get => _selectedSnmpProfile;
        set
        {
            _selectedSnmpProfile = value;
            Device.SnmpProfileId = value?.Id;
            OnPropertyChanged();
        }
    }

    private MountingPoint? _selectedMountingPoint;
    public MountingPoint? SelectedMountingPoint
    {
        get => _selectedMountingPoint;
        set
        {
            if (_selectedMountingPoint == value) return;
            _selectedMountingPoint = value;
            if (Device != null)
                Device.MountingPointId = value?.Id;
            UpdateInstallDateAndMountingPoint();
            OnPropertyChanged();
        }
    }

    private string _lastPollResult = "";
    public string LastPollResult
    {
        get => _lastPollResult;
        set
        {
            _lastPollResult = value;
            OnPropertyChanged();
        }
    }

    private string _installDateAndMountingPoint = "";
    public string InstallDateAndMountingPoint
    {
        get => _installDateAndMountingPoint;
        set
        {
            _installDateAndMountingPoint = value;
            OnPropertyChanged();
        }
    }

    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            OnPropertyChanged(nameof(ErrorMessage));
        }
    }

    private bool _isLoading;

    public bool IsLoading
    {
        get => _isLoading; 
        set 
        { 
            _isLoading = value; 
            OnPropertyChanged();
            PingCommand.RaiseCanExecuteChanged();
            PollSnmpCommand.RaiseCanExecuteChanged();
            ProbeSshCommand.RaiseCanExecuteChanged();
            GetPortsCommand.RaiseCanExecuteChanged();
        }
    }
    
    public string DeviceTitle => Device.Id == 0 ? "Добавление устройства" : $"Редактирование устройства «{Device.Name}»";

    #endregion

    #region ObsevableCollections

    public ObservableCollection<DeviceType> DeviceTypes { get; }
    public ObservableCollection<SnmpProfile> SnmpProfiles { get; }
    public ObservableCollection<MountingPoint> MountingPoints { get; }
    public ObservableCollection<NetworkDevice> ParentCandidates { get; }
    public ObservableCollection<NetworkDevice> Devices { get; }

    private ObservableCollection<DevicePortDto> _devicePorts = new ObservableCollection<DevicePortDto>();
    public ObservableCollection<DevicePortDto> DevicePorts
    {
        get => _devicePorts;
        set { _devicePorts = value; OnPropertyChanged(); }
    }

    #endregion

    #region RelayCommands

    public RelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }
    public RelayCommand PollSnmpCommand { get; }
    public RelayCommand ProbeSshCommand { get; }
    public RelayCommand ClearParentCommand { get; }
    public RelayCommand ManageSnmpProfilesCommand { get; }
    public RelayCommand AddSnmpProfileCommand { get; }
    public RelayCommand PingCommand { get; }
    //public RelayCommand MountingPointDetailsCommand { get; }
    public RelayCommand GetPortsCommand { get; }

    #endregion

    #region Actions

    public Action? GoBack { get; set; }
    public Action<SnmpProfile?>? NavigateAddEditSnmpProfile { get; set; }

    #endregion

    public NetworkDeviceAddEditViewModel(NetworkDevice? device)
    {
        // Инициализация сервисов
        _networkDeviceService = new NetworkDeviceService();
        _poller = new NetworkDevicePoller();
        _messageService = new MessageService();

        // Загрузить актуальные данные устройства из БД
        var sourceDevice = device?.Id > 0
           ? _networkDeviceService.GetDeviceById(device.Id) ?? device
           : device;

        Device = CreateEditableDevice(sourceDevice);

        // Загрузить данные для коллекций, используемых в выпадающих списках
        DeviceTypes = new ObservableCollection<DeviceType>(_networkDeviceService.GetDeviceTypes());
        SnmpProfiles = new ObservableCollection<SnmpProfile>(_networkDeviceService.GetSnmpProfiles());
        ParentCandidates = new ObservableCollection<NetworkDevice>(_networkDeviceService.GetParentCandidates(sourceDevice?.Id));
        
        // Нужно для обновления 
        Devices = new ObservableCollection<NetworkDevice>(_networkDeviceService.GetAllDevices()); 
        MountingPoints = new ObservableCollection<MountingPoint>(_networkDeviceService.GetMountingPointsWithAddress());

        // Установить выбранные элементы в выпадающих списках на основе данных устройства
        SelectedParent = ParentCandidates.FirstOrDefault(p => p.Id == Device?.ParentDeviceId);
        SelectedSnmpProfile = SnmpProfiles.FirstOrDefault(p => p.Id == Device?.SnmpProfileId);
        SelectedMountingPoint = MountingPoints.FirstOrDefault(mp => mp.Id == Device?.MountingPointId);

        UpdateInstallDateAndMountingPoint();

        SaveCommand = new RelayCommand(_ => Save());
        CancelCommand = new RelayCommand(_ => GoBack?.Invoke());
        ClearParentCommand = new RelayCommand(_ => { SelectedParent = null; });
        ManageSnmpProfilesCommand = new RelayCommand(_ => NavigateAddEditSnmpProfile?.Invoke(SelectedSnmpProfile));
        AddSnmpProfileCommand = new RelayCommand(_ => NavigateAddEditSnmpProfile?.Invoke(null));

        // Сетевые команды активируются только при наличии IP адреса и, для SNMP, выбранного профиля, а также неактивны во время выполнения операции (IsLoading), чтобы предотвратить одновременные запросы
        PollSnmpCommand = new RelayCommand(_ => PollSnmpAsync(), _ => (!string.IsNullOrWhiteSpace(Device.IpAddress) && SelectedSnmpProfile != null) && !IsLoading);
        ProbeSshCommand = new RelayCommand(_ => ProbeSshAsync(), _ => !string.IsNullOrWhiteSpace(Device.IpAddress) && !IsLoading);
        PingCommand = new RelayCommand(_ => PollPingAsync(), _ => !string.IsNullOrWhiteSpace(Device.IpAddress) && !IsLoading);
        GetPortsCommand = new RelayCommand(_ => RefreshPortsBySnmp(), _ => (!string.IsNullOrWhiteSpace(Device.IpAddress) && SelectedSnmpProfile != null) && !IsLoading);
    }


    private void Save()
    {
        if (!Validate())
        {
            return;
        }

        try
        {
            Device.DevicePorts.Clear();
            foreach (var dto in DevicePorts)
            {
                Device.DevicePorts.Add(new DevicePort
                {
                    Id = dto.Id,
                    DeviceId = Device.Id,
                    IsUplink = dto.IsUplink,
                    PortName = dto.PortName
                });
            }

            if (Device.Id == 0)
            {
                _networkDeviceService.AddDevice(Device);
                _messageService.Show("Новый образец оборудования успешно добавлен в систему.");
            }
            else
            {
                _networkDeviceService.EditDevice(Device);
                _messageService.Show("Образец оборудования успешно отредактирован.");
            }

            GoBack?.Invoke();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message.ToString();
        }
    }

    // === Validation ===

    private bool Validate()
    {
        var errors = new StringBuilder();
        if (string.IsNullOrWhiteSpace(Device.Name))
            errors.AppendLine("Имя устройства обязательно.");
        else if (Device.Name.Length > 100)
            errors.AppendLine("Имя не длиннее 100 символов.");

        if (string.IsNullOrWhiteSpace(Device.IpAddress))
            errors.AppendLine("IP адрес обязателен.");
        else if (!IPAddress.TryParse(Device.IpAddress.Trim(), out var ip))
            errors.AppendLine("Некорректный IP адрес.");

        if (Device.DeviceTypeId <= 0)
            errors.AppendLine("Выберите тип устройства.");

        if (errors.Length > 0)
        {
            ErrorMessage = errors.ToString().Trim();
            return false;
        }

        ErrorMessage = string.Empty;
        return true;
    }

    // === SNMP/SSH/PING POLLS ===

    private async void PollSnmpAsync()
    {
        LastPollResult = "Получение данных...";
        IsLoading = true;
        try
        {
            var result = await _poller.PollSnmpAsync(Device);
            LastPollResult = result.Message;
        }
        finally
        {
            IsLoading = false;
        }
        //OnPropertyChanged(nameof(LastPollResult));
    }

    private async void RefreshPortsBySnmp()
    {
        LastPollResult = "Получение данных о портах устройства...";

        IsLoading = true;
        try
        {
            var result = await _poller.GetSnmpPortsAsync(Device);
            if (result.Success)
            {
                var dbPorts = _networkDeviceService.GetDevicePorts(Device.Id);

                var finalPorts = new List<DevicePortDto>();

                foreach (var snmpPort in result.Ports)
                {
                    var existingDbPort = dbPorts.FirstOrDefault(p => p.PortName == snmpPort.PortName);

                    if (existingDbPort != null)
                    {
                        finalPorts.Add(new DevicePortDto
                        {
                            Id = existingDbPort.Id,
                            DeviceId = Device.Id,
                            PortName = existingDbPort.PortName ?? "Порт",
                            IsUplink = existingDbPort.IsUplink,
                            OperationalStatus = snmpPort?.OperationalStatus ?? "Неизвестно",
                            StatusRawValue = snmpPort?.StatusRawValue ?? 0
                        });
                    }
                    else
                    {
                        finalPorts.Add(snmpPort);
                    }
                }
                DevicePorts = new ObservableCollection<DevicePortDto>(finalPorts);
            }

            LastPollResult = result.Message;
        }
        finally
        {
            IsLoading = false;
        }
        //OnPropertyChanged(nameof(LastPollResult));
    }

    private async void PollPingAsync()
    {
        LastPollResult = "Загрузка...";
        IsLoading = true;
        try
        {
            var result = await _poller.PollPingAsync(Device);
            LastPollResult = result.Message;
        }
        finally
        {
            IsLoading = false;
        }
        //OnPropertyChanged(nameof(LastPollResult));
    }
    
    private async void ProbeSshAsync()
    {
        LastPollResult = "Загрузка...";
        IsLoading = true;
        try
        {
            var result = await _poller.ProbeSshPortAsync(Device);
            LastPollResult = result.Message;
        }
        finally
        {
            IsLoading = false;
        }
        //OnPropertyChanged(nameof(LastPollResult));
    }

    // === Utils ===

    // Вызывается после перехода на страницу добавления/редактирования SNMP профиля, чтобы обновить данные в случае изменений
    public void Refresh()
    {
        LoadData();
        UpdateInstallDateAndMountingPoint();
    }

    // Вызывается после возвращения с окна добавления/редактирования SNMP профиля, чтобы отобразить возможные изменения в профиле, который может использовать устройство
    private void LoadData()
    {
        var snmpProfiles = _networkDeviceService.GetSnmpProfiles();

        SnmpProfiles.Clear();
        foreach (var profile in snmpProfiles)
            SnmpProfiles.Add(profile); 

        SelectedSnmpProfile = SnmpProfiles.FirstOrDefault(p => p.Id == Device?.SnmpProfileId);
    }

    // Создает новый экземпляр NetworkDevice на основе переданного, для редактирования
    private static NetworkDevice CreateEditableDevice(NetworkDevice? device)
    {
        if (device == null)
        {
            return new NetworkDevice
            {
                IsMonitored = true,
                InstallationDate = null
            };
        }

        var editable = new NetworkDevice
        {
            Id = device.Id,
            Name = device.Name,
            DeviceTypeId = device.DeviceTypeId,
            IpAddress = device.IpAddress,
            MountingPointId = device.MountingPointId,
            ParentDeviceId = device.ParentDeviceId,
            SnmpProfileId = device.SnmpProfileId,
            SnmpProfile = device.SnmpProfile,
            IsMonitored = device.IsMonitored,
            InstallationDate = device.InstallationDate
        };
            foreach (var devicePort in device.DevicePorts)
                editable.DevicePorts.Add(devicePort);

            return editable;
    }

    // Обновляет строку с датой установки и точкой крепления, которая отображается на странице
    private void UpdateInstallDateAndMountingPoint()
    {
        var datePart = Device.InstallationDate.HasValue
            ? $"Дата установки: {Device.InstallationDate.Value:dd.MM.yyyy}"
            : "Дата установки: —";

        var mpPart = SelectedMountingPoint != null
            ? $", Точка крепления: {SelectedMountingPoint.Address?.GetFullAddress}, {SelectedMountingPoint.PointType?.Name ?? "—"}"
            : "";

        InstallDateAndMountingPoint = datePart + mpPart;
    }
}
// O_o