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

    private void LoadData()
    {
        var addr = _addressService.GetAddressesWithInclude().FirstOrDefault(a => a.Id == _addressId);

        if (addr == null) return;

        var city = addr.Street?.City?.Name ?? "—";
        var street = addr.Street?.Name ?? "—";
        FullAddress = $"{city}, {street}, д. {addr.HouseNumber}";

        MountingPoints.Clear();
        foreach (var mp in addr.MountingPoints)
        {
            MountingPoints.Add(mp);
        }

        var allDevices = addr.MountingPoints.SelectMany(mp => mp.NetworkDevices).ToList();
        Devices.Clear();
        foreach (var dev in allDevices)
        {
            Devices.Add(dev);
        }
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
            _messageService.ShowError(ex.Message);
        }
    }

    private void UnbindSelectedDevice(object? param)
    {
        if (param is not NetworkDevice device) return;

        if (!_messageService.Confirm($"Отвязать устройство \"{device.Name}\" от точки монтажа?"))
            return;

        try
        {
           if (device != null)
           {
                device.MountingPointId = null;
                device.InstallationDate = null;
                _deviceService.EditDevice(device);
           }
           LoadData();
        }
        catch (Exception ex) 
        { 
            _messageService.ShowError(ex.Message); 
        }
    }

    public void Refresh()
    {
        LoadData();
    }
}