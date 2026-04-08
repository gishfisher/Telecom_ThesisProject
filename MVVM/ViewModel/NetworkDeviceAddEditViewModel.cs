using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Telecom_ThesisProject.Core;
using Telecom_ThesisProject.MVVM.Model;
using Telecom_ThesisProject.Services;
using Telecom_ThesisProject.Utilities;
using Telecom_ThesisProject.Utilities.Interfaces;

namespace Telecom_ThesisProject.MVVM.ViewModel;

class NetworkDeviceAddEditViewModel : ObservableObject
{
    private readonly NetworkDeviceService _networkDeviceService;
    private readonly NetworkLabOptions _lab;
    private readonly INetworkDevicePoller _poller;
    private readonly IMessageService _messageService;

    public NetworkDevice Device { get; }

    public ObservableCollection<DeviceType> DeviceTypes { get; }
    public ObservableCollection<MountingPoint> MountingPoints { get; }
    public ObservableCollection<NetworkDevice> ParentCandidates { get; }

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

    public DateTime? InstallDateUi
    {
        get => Device.InstallationDate.HasValue
            ? Device.InstallationDate.Value.ToDateTime(TimeOnly.MinValue)
            : null;
        set
        {
            Device.InstallationDate = value.HasValue ? DateOnly.FromDateTime(value.Value) : null;
            OnPropertyChanged();
        }
    }

    //public string LabInfoText { get; }

    public string LastPollResult
    {
        get => _lastPollResult;
        set
        {
            _lastPollResult = value;
            OnPropertyChanged();
        }
    }

    private string _lastPollResult = "";

    public RelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }
    public RelayCommand PollSnmpCommand { get; }
    public RelayCommand ProbeSshCommand { get; }
    public RelayCommand ClearParentCommand { get; }

    public Action? GoBack { get; set; }

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

    public NetworkDeviceAddEditViewModel(NetworkDevice? selected)
    {
        _messageService = new MessageService();
        _networkDeviceService = new NetworkDeviceService();
        _poller = new SnmpNetworkDevicePoller();
        _lab = NetworkLabSettings.Load();

        //LabInfoText =
        //    $"{_lab.Description}\n{_lab.TopologyNote}\nУправляющая сеть (Cloud0 / lab): {_lab.ManagementNetworkCidr}";

        Device = CreateEditableDevice(selected);

        DeviceTypes = new ObservableCollection<DeviceType>(_networkDeviceService.GetDeviceTypes());
        MountingPoints = new ObservableCollection<MountingPoint>(_networkDeviceService.GetMountingPointsWithAddress());
        ParentCandidates = new ObservableCollection<NetworkDevice>(_networkDeviceService.GetParentCandidates(selected?.Id));

        _selectedParent = ParentCandidates.FirstOrDefault(p => p.Id == Device.ParentDeviceId);
        OnPropertyChanged(nameof(SelectedParent));
        OnPropertyChanged(nameof(InstallDateUi));

        SaveCommand = new RelayCommand(_ => Save());
        CancelCommand = new RelayCommand(_ => GoBack?.Invoke());
        PollSnmpCommand = new RelayCommand(_ => PollSnmp());
        ProbeSshCommand = new RelayCommand(_ => ProbeSsh());
        ClearParentCommand = new RelayCommand(_ => { SelectedParent = null; });
    }

    private void PollSnmp()
    {
        var r = _poller.PollSnmp(Device);
        LastPollResult = r.Message;
    }

    private void ProbeSsh()
    {
        _poller.TryProbeSshPort(Device.IpAddress, out var msg);
        LastPollResult = string.IsNullOrEmpty(LastPollResult) ? msg : LastPollResult + "\n\n" + msg;
        OnPropertyChanged(nameof(LastPollResult));
    }

    private void Save()
    {
        if (!Validate())
        {
            _messageService.Show(ErrorMessage);
            return;
        }

        try
        {
            if (Device.Id == 0)
                _networkDeviceService.AddDevice(Device);
            else
                _networkDeviceService.EditDevice(Device);

            GoBack?.Invoke();
        }
        catch (Exception ex)
        {
            _messageService.Show(ex.Message);
        }
    }

    private bool Validate()
    {
        var errors = new StringBuilder();
        if (string.IsNullOrWhiteSpace(Device.Name))
            errors.AppendLine("Имя устройства обязательно.");
        else if (Device.Name.Length > 100)
            errors.AppendLine("Имя не длиннее 100 символов.");

        if (string.IsNullOrWhiteSpace(Device.IpAddress))
            errors.AppendLine("IP адрес обязателен.");
        else if (Device.IpAddress.Length > 45)
            errors.AppendLine("IP слишком длинный.");

        if (!string.IsNullOrWhiteSpace(Device.SnmpCommunity) && Device.SnmpCommunity.Length > 50)
            errors.AppendLine("SNMP community не длиннее 50 символов.");

        if (Device.DeviceTypeId <= 0)
            errors.AppendLine("Выберите тип устройства.");

        if (Device.MountingPointId <= 0)
            errors.AppendLine("Выберите точку монтажа.");

        if (errors.Length > 0)
        {
            ErrorMessage = errors.ToString().Trim();
            return false;
        }

        ErrorMessage = string.Empty;
        return true;
    }

    private static NetworkDevice CreateEditableDevice(NetworkDevice? d)
    {
        if (d == null)
        {
            return new NetworkDevice
            {
                IsMonitored = true,
                SnmpCommunity = "public",
                InstallationDate = DateOnly.FromDateTime(DateTime.Today)
            };
        }

        return new NetworkDevice
        {
            Id = d.Id,
            Name = d.Name,
            DeviceTypeId = d.DeviceTypeId,
            Vendor = d.Vendor,
            Model = d.Model,
            SerialNumber = d.SerialNumber,
            IpAddress = d.IpAddress,
            SnmpCommunity = d.SnmpCommunity,
            MountingPointId = d.MountingPointId,
            ParentDeviceId = d.ParentDeviceId,
            IsMonitored = d.IsMonitored,
            InstallationDate = d.InstallationDate,
        };
    }
}
