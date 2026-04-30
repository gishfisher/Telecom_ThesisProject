using Microsoft.EntityFrameworkCore;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;
using Renci.SshNet.Security;
using Renci.SshNet.Security.Cryptography;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Controls.Primitives;
using Telecom_ThesisProject.Core;
using Telecom_ThesisProject.Data;
using Telecom_ThesisProject.MVVM.Model;
using Telecom_ThesisProject.Services;
using Telecom_ThesisProject.Utilities;
using Telecom_ThesisProject.Utilities.DTOs;
using Telecom_ThesisProject.Utilities.Interfaces;

namespace Telecom_ThesisProject.MVVM.ViewModel;

public class NetworkDeviceDetailsViewModel : ObservableObject
{
    private readonly NetworkDeviceService _networkDeviceService;
    private readonly NetworkLabOptions _options;
    private readonly ConnectionService _connectionService;
    private readonly INetworkDevicePoller _poller;
    private readonly IMessageService _messageService;

    #region Properties

    private NetworkDevice _device;
    public NetworkDevice Device
    {
        get => _device;
        private set { _device = value; OnPropertyChanged(); }
    }

    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            OnPropertyChanged();
        }
    }

    private string _deviceUpTime = string.Empty;
    public string DeviceUpTime
    {
        get => _deviceUpTime;
        set 
        {
            _deviceUpTime = value; 
            OnPropertyChanged(nameof(DeviceUpTime));
        }
    }

    private string _deviceStatus = string.Empty;
    public string DeviceStatus
    {
        get => _deviceStatus;
        set 
        { 
            _deviceStatus = value; 
            OnPropertyChanged(nameof(DeviceStatus));
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
            RefreshDeviceDetails.RaiseCanExecuteChanged();
        }
    }

    private DevicePortDto _selectedPort;

    public DevicePortDto SelectedPort
    {
        get => _selectedPort;
        set
        {
            _selectedPort = value;
            OnPropertyChanged();
        }
    }

    #endregion

    private ObservableCollection<DevicePortDto> _devicePorts = [];
    public ObservableCollection<DevicePortDto> DevicePorts
    {
        get => _devicePorts;
        set
        {
            _devicePorts = value;
            OnPropertyChanged();
        }
    }

    public RelayCommand GoBackCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand EditConnectionCommand { get; }
    public RelayCommand AddConnectionCommand { get; }
    public RelayCommand OpenSshCommand { get; }
    public RelayCommand ManageSnmpProfilesCommand { get; }
    public RelayCommand ViewParentDeviceDetailsCommand { get; }
    public RelayCommand RefreshDeviceDetails { get; }

    public Action? GoBack { get; set; }
    public Action<NetworkDevice>? NavigateEditDevice { get; set; }
    public Action<SnmpProfile>? NavigateAddEditSnmpProfile { get; set; }
    public Action<NetworkDevice>? NavigateToParentDeviceDetails { get; set; }
    public Action<Connection?, DevicePortDto?>? NavigateToAddEditConnection { get; set; } = null;

    public NetworkDeviceDetailsViewModel(NetworkDevice device)
    {
        _networkDeviceService = new NetworkDeviceService();
        _connectionService = new ConnectionService();
        _poller = new NetworkDevicePoller();
        _messageService = new MessageService();
        _options = new NetworkLabOptions();

        Device = _networkDeviceService.GetDeviceById(device.Id) ?? device;

        GoBackCommand = new RelayCommand(_ => GoBack?.Invoke());
        EditCommand = new RelayCommand(_ => NavigateEditDevice?.Invoke(Device));
        OpenSshCommand = new RelayCommand(_ => OpenSsh());
        ManageSnmpProfilesCommand = new RelayCommand(_ => NavigateAddEditSnmpProfile?.Invoke(Device.SnmpProfile));
        ViewParentDeviceDetailsCommand = new RelayCommand(_ => NavigateToParentDeviceDetails?.Invoke(Device.ParentDevice), _ => Device.ParentDevice != null);
        RefreshDeviceDetails = new RelayCommand(_ => Refresh(), _ => !IsLoading);

        // Команды для управления соединениями
        AddConnectionCommand = new RelayCommand(_ => AddEditConnection(SelectedPort));
        EditConnectionCommand = new RelayCommand(_ => AddEditConnection(SelectedPort));

        // Загружаем данные устройства при инициализации ViewModel
        _ = LoadDataAsync();  
    }

    private async Task LoadDataAsync()
    {
        // Загружаем свежие данные устройства, чтобы получить актуальные порты и связи
        var fresh = _networkDeviceService.GetDeviceById(Device.Id);
        if (fresh != null)
            Device = fresh;

        // Обновляем список портов, преобразуя их в DTO (Временный объект данных) для отображения
        DevicePorts.Clear();
        foreach (var port in Device.DevicePorts)
        {
            DevicePorts.Add(new DevicePortDto
            {
                Id = port?.Id ?? 0,
                DeviceId = port?.DeviceId ?? 0,
                IsUplink = port?.IsUplink ?? false,
                PortName = port?.PortName ?? string.Empty,
                OperationalStatus = "Неизвестно",
                HasConnection = port?.Connection != null,
                Connection = port?.Connection ?? null
            });
        }

        IsLoading = true;
        try
        {
            await Task.WhenAll(
                GetDeviceStatusAsync(),
                GetUpTimeAsync(),
                RefreshPortsAsync()
            );
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void AddEditConnection(DevicePortDto selectedPort)
    {
        if (selectedPort?.HasConnection == false)
        {
            NavigateToAddEditConnection?.Invoke(null, selectedPort);
        }

        if (selectedPort?.HasConnection == true)
        {
            var connection = _connectionService.GetAllConnections()
                .FirstOrDefault(c => c.Id == selectedPort.Connection?.Id);

            NavigateToAddEditConnection?.Invoke(connection, selectedPort);
        }
    }

    public void Refresh()
    {
        _ = LoadDataAsync();
    }

    // Временные методы для получения статуса устройства и времени работы через SNMP и ICMP
    private async Task GetDeviceStatusAsync()
    {
        DeviceStatus = "Загрузка...";

        var result = await _poller.PollPingAsync(Device);

        if (result.Success)
        {
            DeviceStatus = "В сети";
        }
        else
        {
            DeviceStatus = "Недоступен";
        }
    }

    private async Task GetUpTimeAsync()
    {
        DeviceUpTime = "Загрузка...";

        var result = await _poller.PollSnmpUpTimeAsync(Device);

        if (result.Success)
        {
            DeviceUpTime = result.UptimeStatus;
        }
        else
        {
            DeviceUpTime = "Недоступен";
        }
    }

    private async Task RefreshPortsAsync()
    {
        ErrorMessage = "Загрузка...";

        var result = await _poller.GetSnmpPortsAsync(Device);

        if (!result.Success)
        {
            ErrorMessage = "Ошибка загрузки интерфейсов.";
            return;
        }

        foreach (var snmpPort in result.Ports)
        {
            var existing = DevicePorts.FirstOrDefault(p => p.PortName == snmpPort.PortName);
            if (existing != null)
            {
                existing.OperationalStatus = snmpPort?.OperationalStatus ?? "Неизвестно.";
                existing.StatusRawValue = snmpPort?.StatusRawValue ?? 0;
            }
        }

        ErrorMessage = "Данные успешно обновлены.";

        await Task.Delay(3000);
        ErrorMessage = string.Empty;
    }

    private void OpenSsh()
    {
        try
        {
            string sshCommand = $"ssh admin@{Device.IpAddress} -p {_options.SshPort}";

            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/k echo Подключение к {Device.IpAddress} && {sshCommand}",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            _messageService.Show($"Ошибка запуска CMD: {ex.Message}");
        }
    }
}
