using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Renci.SshNet;
using Renci.SshNet.Common;
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

    //public ObservableCollection<PortRowViewModel> Ports { get; } = new();
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
    public RelayCommand SaveCommand { get; }
    public RelayCommand RefreshPortsCommand { get; }
    public RelayCommand OpenSshCommand { get; }
    public RelayCommand RebootCommand { get; }

    public Action? GoBack { get; set; }
    public Action<NetworkDevice>? NavigateEditDevice { get; set; }

    public NetworkDeviceDetailsViewModel(NetworkDevice device)
    {
        _networkDeviceService = new NetworkDeviceService();
        _clientService = new ClientService();
        _poller = new SnmpNetworkDevicePoller();
        _messageService = new MessageService();
        _lab = NetworkLabSettings.Load();

        Device = device;

        GoBackCommand = new RelayCommand(_ => GoBack?.Invoke());
        EditCommand = new RelayCommand(_ => NavigateEditDevice?.Invoke(Device));
        SaveCommand = new RelayCommand(_ => SaveConfig());
        RefreshPortsCommand = new RelayCommand(_ => LoadPorts());
        OpenSshCommand = new RelayCommand(_ => OpenSsh());
        RebootCommand = new RelayCommand(_ => RebootDevice());

        LoadData();
    }

    private void LoadData()
    {
        using var db = new TelecomDbContext();

        var device = db.NetworkDevices
            .Include(d => d.DevicePorts)
                .ThenInclude(p => p.Connection)
                    .ThenInclude(c => c.Client)
            .FirstOrDefault(d => d.Id == Device.Id);

        if (device == null) return;

        //Ports.Clear();
        foreach (var port in device.DevicePorts)
        {
            db.Entry(port).State = EntityState.Detached;
            if (port.Connection?.Client != null)
                db.Entry(port.Connection.Client).State = EntityState.Detached;
            //var row = new PortRowViewModel(port);
            //row.SelectedClient = port.Connection?.Client;
            //Ports.Add(row);
        }

        var clients = _clientService.GetAll();
        ClientsList.Clear();
        foreach (var c in clients) ClientsList.Add(c);

        var parents = _networkDeviceService.GetParentCandidates(Device.Id);
        ParentDevices.Clear();
        foreach (var p in parents) ParentDevices.Add(p);
        SelectedParentDevice = parents.FirstOrDefault(p => p.Id == Device.ParentDeviceId);
    }

    private void LoadPorts()
    {
        LoadData();
        _messageService.Show("Порты обновлены.");
    }

    private void SaveConfig()
    {
        try
        {
            using var db = new TelecomDbContext();
            var existing = db.NetworkDevices.FirstOrDefault(d => d.Id == Device.Id);
            if (existing == null)
            {
                _messageService.Show("Устройство не найдено.");
                return;
            }

            existing.SnmpCommunity = Device.SnmpCommunity;
            existing.ParentDeviceId = SelectedParentDevice?.Id;
            db.SaveChanges();

            //foreach (var row in Ports)
            //{
            //    var port = db.DevicePorts.FirstOrDefault(p => p.Id == row.PortNumber);
            //    if (port == null) continue;

            //    if (row.SelectedClient != null && !port.IsUplink)
            //    {
            //        var conn = port.Connection;
            //        if (conn == null)
            //        {
            //            conn = new Connection
            //            {
            //                PortId = port.Id,
            //                ClientId = row.SelectedClient.Id,
            //                TariffId = 1,
            //                IsActive = true
            //            };
            //            db.Connections.Add(conn);
            //        }
            //        else
            //        {
            //            conn.ClientId = row.SelectedClient.Id;
            //        }
            //    }
            //}
            db.SaveChanges();

            _messageService.Show("Конфигурация сохранена.");
        }
        catch (Exception ex)
        {
            _messageService.Show(ex.Message);
        }
    }

    private void OpenSsh()
    {
        try
        {
            var keyboardAuth = new KeyboardInteractiveAuthenticationMethod("admin");
            var passwordAuth = new PasswordAuthenticationMethod("admin", "admin");

            var connectionInfo = new ConnectionInfo(Device.IpAddress, _lab.SshPort, "admin", passwordAuth, keyboardAuth)
            {
                Timeout = TimeSpan.FromSeconds(10)
            };

            using var client = new SshClient(connectionInfo);
            client.Connect();

            if (client.IsConnected)
            {
                _messageService.Show($"SSH подключено к {Device.IpAddress}");
                client.Disconnect();
            }
        }
        catch (SshConnectionException ex)
        {
            _messageService.Show($"SSH ошибка подключения: {ex.Message}\n\nВозможно, устройство не поддерживает текущие алгоритмы обмена ключами.");
        }
        catch (Exception ex)
        {
            _messageService.Show($"SSH ошибка: {ex.Message}");
        }
    }

    private void RebootDevice()
    {
        if (_messageService.Confirm($"Перезагрузить устройство {Device.Name} ({Device.IpAddress})?"))
        {
            _messageService.Show($"Команда перезагрузки отправлена на {Device.IpAddress}");
        }
    }

    public void Refresh()
    {
        LoadData();
    }
}
