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
    private readonly AddressService _addressService;
    private readonly INetworkDevicePoller _poller;
    private readonly IMessageService _messageService;

    #region Obsevable collections for dropdowns

        public ObservableCollection<DeviceType> DeviceTypes { get; }
        public ObservableCollection<SnmpProfile> SnmpProfiles { get; } 
        public ObservableCollection<MountingPoint> MountingPoints { get; }
        public ObservableCollection<NetworkDevice> ParentCandidates { get; }

    #endregion

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
            _selectedMountingPoint = value;
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

    private string _installDateAndMountingPoint;
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

    #endregion

    #region RelayCommands

        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }
        public RelayCommand PollSnmpCommand { get; }
        public RelayCommand ProbeSshCommand { get; }
        public RelayCommand ClearParentCommand { get; }
        public RelayCommand ManageSnmpProfilesCommand { get; }
        public RelayCommand AddSnmpProfileCommand { get; }

    #endregion
    
    public string DeviceTitle => Device.Id == 0 ? "Добавление устройства" : $"Редактирование устройства «{Device.Name}»";
    public Action? GoBack { get; set; }
    public Action<SnmpProfile> NavigateAddEditSnmpProfile { get; set; }

    public NetworkDeviceAddEditViewModel(NetworkDevice? selected)
    {
        _messageService = new MessageService();
        _networkDeviceService = new NetworkDeviceService();
        _addressService = new AddressService();
        _poller = new SnmpNetworkDevicePoller();

        var sourceDevice = selected;
        if (selected?.Id > 0)
            sourceDevice = _networkDeviceService.GetDeviceById(selected.Id) ?? selected;

        Device = CreateEditableDevice(sourceDevice);

        DeviceTypes = new ObservableCollection<DeviceType>(_networkDeviceService.GetDeviceTypes());
        SnmpProfiles = new ObservableCollection<SnmpProfile>(_networkDeviceService.GetSnmpProfiles());
        MountingPoints = new ObservableCollection<MountingPoint>(_networkDeviceService.GetMountingPointsWithAddress());
        ParentCandidates = new ObservableCollection<NetworkDevice>(_networkDeviceService.GetParentCandidates(sourceDevice?.Id));

        _selectedParent = ParentCandidates.FirstOrDefault(p => p.Id == Device.ParentDeviceId);
        OnPropertyChanged(nameof(SelectedParent));

        _selectedSnmpProfile = SnmpProfiles.FirstOrDefault(p => p.Id == Device.SnmpProfileId);
        OnPropertyChanged(nameof(SelectedSnmpProfile));

        SelectedMountingPoint = MountingPoints.FirstOrDefault(mp => mp.Id == Device.MountingPointId);

        UpdateInstallDateAndMountingPoint();

        SaveCommand = new RelayCommand(_ => Save());
        CancelCommand = new RelayCommand(_ => GoBack?.Invoke());
        PollSnmpCommand = new RelayCommand(_ => PollSnmp());
        ProbeSshCommand = new RelayCommand(_ => ProbeSsh());
        ClearParentCommand = new RelayCommand(_ => { SelectedParent = null; });
        ManageSnmpProfilesCommand = new RelayCommand(_ => NavigateAddEditSnmpProfile?.Invoke(SelectedSnmpProfile!));
        AddSnmpProfileCommand = new RelayCommand(_ => NavigateAddEditSnmpProfile?.Invoke(SelectedSnmpProfile = null!));
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
            UpdateInstallDateAndMountingPoint();

            if (Device.Id == 0)
                _networkDeviceService.AddDevice(Device);
            else
                _networkDeviceService.EditDevice(Device);

            UpdateInstallDateAndMountingPoint();

            GoBack?.Invoke();
        }
        catch (Exception ex)
        {
            _messageService.Show(ex.Message);
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
        else if (Device.IpAddress.Length > 45)
            errors.AppendLine("IP слишком длинный.");

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

    // === SNMP/SSH POLL ===

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

    // === Utils ===

    // Called after adding/editing SNMP profile in the separate view to update the list of profiles in the dropdown
    public void Refresh()
    {
        LoadSnmpProfiles();
    }

    // Load SNMP Profiles to update the list after adding/editing a profile in the separate view
    private void LoadSnmpProfiles()
    {
        var list = _networkDeviceService.GetSnmpProfiles();
        SnmpProfiles.Clear();
        foreach (var d in list)
            SnmpProfiles.Add(d);
        OnPropertyChanged(nameof(SnmpProfiles));
    }

    // Create a copy of the device to edit, so that changes are not applied to the original until saving
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

        return new NetworkDevice
        {
            Id = device.Id,
            Name = device.Name,
            DeviceTypeId = device.DeviceTypeId,
            IpAddress = device.IpAddress,
            MountingPointId = device.MountingPointId,
            ParentDeviceId = device.ParentDeviceId,
            SnmpProfileId = device.SnmpProfileId,
            IsMonitored = device.IsMonitored,
            InstallationDate = device.InstallationDate,
        };
    }

    // Update the display string for installation date and mounting point based on current device data
    private void UpdateInstallDateAndMountingPoint()
    {
        var datePart = Device.InstallationDate.HasValue
            ? $"Дата установки: {Device.InstallationDate.Value.ToString("dd.MM.yyyy")}"
            : "Дата установки: —";

        var mpPart = SelectedMountingPoint != null
            ? $", Точка крепления: {SelectedMountingPoint.Address?.GetFullAddress ?? "—"}"
            : "";

        InstallDateAndMountingPoint = datePart + mpPart;
    }
}
