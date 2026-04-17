using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using Telecom_ThesisProject.Core;
using Telecom_ThesisProject.Data;
using Telecom_ThesisProject.MVVM.Model;
using Telecom_ThesisProject.Services;
using Telecom_ThesisProject.Utilities.Interfaces;

namespace Telecom_ThesisProject.MVVM.ViewModel;

public class AddressDetailsViewModel : ObservableObject
{
    private readonly AddressService _addressService;
    private readonly NetworkDeviceService _deviceService;
    private readonly IMessageService _messageService;
    private readonly int _addressId;

    private string _fullAddress = string.Empty;
    public string FullAddress
    {
        get => _fullAddress;
        set { _fullAddress = value; OnPropertyChanged(); }
    }

    public ObservableCollection<MountingPoint> MountingPoints { get; } = new();
    public ObservableCollection<NetworkDevice> Devices { get; } = new();

    //public ObservableCollection<DevicePort> Ports { get; } = new();

    private object? _selectedMountingPoint;
    public object? SelectedMountingPoint
    {
        get => _selectedMountingPoint;
        set { _selectedMountingPoint = value; OnPropertyChanged(); }
    }

    private NetworkDevice? _selectedDevice;
    public NetworkDevice? SelectedDevice
    {
        get => _selectedDevice;
        set { _selectedDevice = value; OnPropertyChanged(); }
    }

    public RelayCommand GoBackCommand { get; }
    public RelayCommand AddMountingPointCommand { get; }
    public RelayCommand DeleteMountingPointCommand { get; }
    public RelayCommand AddDeviceCommand { get; }
    public RelayCommand UnbindDeviceCommand { get; }
    public RelayCommand EditMountingPointCommand { get; }
    public RelayCommand OpenDeviceDetailsCommand { get; }

    public Action? GoBack { get; set; }
    public Action<MountingPoint?>? NavigateAddEditMountingPoint { get; set; }
    public Action<int>? NavigateBindDevice { get; set; }
    public Action<NetworkDevice>? NavigateDeviceDetails { get; set; }
    public Action<NetworkDevice>? NavigateEditDevice { get; set; }

    public AddressDetailsViewModel(Address address)
    {
        _addressService = new AddressService();
        _messageService = new MessageService();
        _deviceService = new NetworkDeviceService();
        _addressId = address.Id;

        GoBackCommand = new RelayCommand(_ => GoBack?.Invoke());
        AddMountingPointCommand = new RelayCommand(_ => NavigateAddEditMountingPoint?.Invoke(null));
        EditMountingPointCommand = new RelayCommand(_ => NavigateAddEditMountingPoint?.Invoke(SelectedMountingPoint as MountingPoint), _ => SelectedMountingPoint is MountingPoint);
        DeleteMountingPointCommand = new RelayCommand(DeleteSelectedMountingPoint, _ => SelectedMountingPoint is MountingPoint);
        AddDeviceCommand = new RelayCommand(_ => NavigateBindDevice?.Invoke(_addressId));
        UnbindDeviceCommand = new RelayCommand(UnbindSelectedDevice, _ => SelectedDevice != null);
        OpenDeviceDetailsCommand = new RelayCommand(_ =>
        {
            if (SelectedDevice != null)
                NavigateDeviceDetails?.Invoke(SelectedDevice);
        }, _ => SelectedDevice != null);

        LoadData();
    }

    //private void LoadData()
    //{
    //    using var db = new TelecomDbContext();

    //    var addr = db.Addresses
    //        .Include(a => a.Street).ThenInclude(s => s.City)
    //        .Include(a => a.MountingPoints)
    //            .ThenInclude(mp => mp.PointType)
    //        .Include(a => a.MountingPoints)
    //            .ThenInclude(mp => mp.NetworkDevices)
    //                .ThenInclude(d => d.DeviceType)
    //        .Include(a => a.MountingPoints)
    //            .ThenInclude(mp => mp.NetworkDevices)
    //                .ThenInclude(d => d.DevicePorts)
    //                    .ThenInclude(p => p.Connection)
    //                        .ThenInclude(c => c.Client)
    //        .Include(a => a.MountingPoints)
    //            .ThenInclude(mp => mp.NetworkDevices)
    //                .ThenInclude(d => d.DevicePorts)
    //                    .ThenInclude(p => p.Connection)
    //                        .ThenInclude(c => c.Tariff)
    //        .FirstOrDefault(a => a.Id == _addressId);

    //    if (addr == null) return;

    //    var city = addr.Street?.City?.Name ?? "—";
    //    var street = addr.Street?.Name ?? "—";
    //    FullAddress = $"{city}, {street}, д. {addr.HouseNumber}" +
    //                  (!string.IsNullOrWhiteSpace(addr.Apartment) ? $", кв. {addr.Apartment}" : "");

    //    MountingPoints.Clear();
    //    foreach (var mp in addr.MountingPoints)
    //    {
    //        db.Entry(mp).State = EntityState.Detached;
    //        if (mp.PointType != null) db.Entry(mp.PointType).State = EntityState.Detached;
    //        MountingPoints.Add(mp);
    //    }

    //    Devices.Clear();
    //    foreach (var mp in addr.MountingPoints)
    //    {
    //        foreach (var device in mp.NetworkDevices)
    //        {
    //            db.Entry(device).State = EntityState.Detached;
    //            if (device.DeviceType != null) db.Entry(device.DeviceType).State = EntityState.Detached;
    //            Devices.Add(device);
    //        }
    //    }

    //    Ports.Clear();
    //    foreach (var mp in addr.MountingPoints)
    //    {
    //        foreach (var device in mp.NetworkDevices)
    //        {
    //            foreach (var port in device.DevicePorts)
    //            {
    //                db.Entry(port).State = EntityState.Detached;
    //                if (port.Connection?.Client != null) db.Entry(port.Connection.Client).State = EntityState.Detached;
    //                if (port.Connection?.Tariff != null) db.Entry(port.Connection.Tariff).State = EntityState.Detached;
    //                Ports.Add(port);
    //            }
    //        }
    //    }
    //}

    private void LoadData()
    {
        using var db = new TelecomDbContext();

        var addr = db.Addresses
            .AsNoTracking()
            .AsSplitQuery()
            .Include(a => a.Street).ThenInclude(s => s.City)
            .Include(a => a.MountingPoints)
                .ThenInclude(mp => mp.PointType)
            .Include(a => a.MountingPoints)
                .ThenInclude(mp => mp.NetworkDevices)
                    .ThenInclude(d => d.DeviceType)
            .Include(a => a.MountingPoints)
                .ThenInclude(mp => mp.NetworkDevices)
                    .ThenInclude(d => d.DevicePorts)
                        .ThenInclude(p => p.Connection)
                            .ThenInclude(c => c.Client)
            .Include(a => a.MountingPoints)
                .ThenInclude(mp => mp.NetworkDevices)
                    .ThenInclude(d => d.DevicePorts)
                        .ThenInclude(p => p.Connection)
                            .ThenInclude(c => c.Tariff)
            .FirstOrDefault(a => a.Id == _addressId);

        if (addr == null) return;

        var city = addr.Street?.City?.Name ?? "—";
        var street = addr.Street?.Name ?? "—";
        FullAddress = $"{city}, {street}, д. {addr.HouseNumber}";

        MountingPoints.Clear();
        foreach (var mp in addr.MountingPoints) MountingPoints.Add(mp);

        var allDevices = addr.MountingPoints.SelectMany(mp => mp.NetworkDevices).ToList();
        Devices.Clear();
        foreach (var dev in allDevices) Devices.Add(dev);

        //var allPorts = allDevices.SelectMany(d => d.DevicePorts).ToList();
        //Ports.Clear();
        //foreach (var port in allPorts) Ports.Add(port);
    }

    private void DeleteSelectedMountingPoint(object? param)
    {
        if (param is not MountingPoint mp) return;

        if (!_messageService.Confirm($"Удалить точку монтажа \"{mp.PointType?.Name}\" и всё оборудование?"))
            return;

        try
        {
            _addressService.RemoveMountingPoint(mp);
            MountingPoints.Remove(mp);
            LoadData();
        }
        catch (Exception ex)
        {
            _messageService.Show(ex.Message);
        }
    }

    private void UnbindSelectedDevice(object? param)
    {
        if (param is not NetworkDevice device) return;

        if (!_messageService.Confirm($"Отвязать устройство \"{device.Name}\" от точки монтажа? Устройство останется в базе."))
            return;

        try
        {
            using var db = new TelecomDbContext();
            var existing = db.NetworkDevices.FirstOrDefault(d => d.Id == device.Id);
            if (existing != null)
            {
                existing.MountingPointId = null;
                existing.InstallationDate = null;
                db.SaveChanges();
            }
            LoadData();
        }
        catch (Exception ex) 
        { 
            _messageService.Show(ex.Message); 
        }
    }

    public void Refresh()
    {
        LoadData();
    }
}