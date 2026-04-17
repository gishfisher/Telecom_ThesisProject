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
using Telecom_ThesisProject.Utilities.Interfaces;

namespace Telecom_ThesisProject.MVVM.ViewModel;

public class NetworkDeviceDetailsViewModel : ObservableObject
{
    private readonly NetworkDeviceService _networkDeviceService;
    private readonly ClientService _clientService;
    private readonly INetworkDevicePoller _poller;
    private readonly IMessageService _messageService;
    private readonly NetworkLabOptions _lab;

    public NetworkDevice Device { get; }

    public ObservableCollection<Client> ClientsList { get; } = new();
    public ObservableCollection<NetworkDevice> ParentDevices { get; } = new();

    private NetworkDevice? _selectedParentDevice;
    public NetworkDevice? SelectedParentDevice
    {
        get => _selectedParentDevice;
        set { _selectedParentDevice = value; OnPropertyChanged(); }
    }

    public RelayCommand GoBackCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand RefreshPortsCommand { get; }
    public RelayCommand OpenSshCommand { get; }

    public Action? GoBack { get; set; }
    public Action<NetworkDevice>? NavigateEditDevice { get; set; }

    public NetworkDeviceDetailsViewModel(NetworkDevice device)
    {
        _networkDeviceService = new NetworkDeviceService();
        _clientService = new ClientService();
        _poller = new SnmpNetworkDevicePoller();
        _messageService = new MessageService();
        _lab = NetworkLabSettings.Load();

        var sourceDevice = device;
        if (device?.Id > 0)
            sourceDevice = _networkDeviceService.GetDeviceById(device.Id) ?? device;

        Device = sourceDevice;

        GoBackCommand = new RelayCommand(_ => GoBack?.Invoke());
        EditCommand = new RelayCommand(_ => NavigateEditDevice?.Invoke(Device));
        OpenSshCommand = new RelayCommand(_ => OpenSsh());

        LoadData();
    }

    private void LoadData()
    {
        using var db = new TelecomDbContext();

        var freshWithPorts = db.NetworkDevices
            .AsNoTracking()
            .Include(d => d.DevicePorts)
                .ThenInclude(p => p.Connection)
                    .ThenInclude(c => c.Client)
            .FirstOrDefault(d => d.Id == Device.Id);

        if (freshWithPorts == null) return;

        UpdateDeviceFrom(freshWithPorts);

        ClientsList.Clear();
        foreach (var c in _clientService.GetAll()) ClientsList.Add(c);

        ParentDevices.Clear();
        var parents = _networkDeviceService.GetParentCandidates(Device.Id);
        foreach (var p in parents) ParentDevices.Add(p);
        SelectedParentDevice = parents.FirstOrDefault(p => p.Id == Device.ParentDeviceId);
    }

    private void UpdateDeviceFrom(NetworkDevice source)
    {
        if (source == null) return;

        Device.Name = source.Name;
        Device.IpAddress = source.IpAddress;
        Device.DeviceTypeId = source.DeviceTypeId;
        Device.ParentDeviceId = source.ParentDeviceId;
        Device.SnmpProfileId = source.SnmpProfileId;
        Device.IsMonitored = source.IsMonitored;
        Device.InstallationDate = source.InstallationDate;

        OnPropertyChanged(nameof(Device));
    }

    private void OpenSsh()
    {
        try
        {
            string sshCommand = $"ssh admin@{Device.IpAddress} -p {_lab.SshPort}";

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

    public void Refresh()
    {
        LoadData();
    }
}
