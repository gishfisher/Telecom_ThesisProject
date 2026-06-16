using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using Telecom_ThesisProject.Core;
using Telecom_ThesisProject.Data;
using Telecom_ThesisProject.MVVM.Model;
using Telecom_ThesisProject.Services;
using Telecom_ThesisProject.Utilities.Interfaces;

namespace Telecom_ThesisProject.MVVM.ViewModel;

public class BindDeviceViewModel : ObservableObject
{
    private readonly AddressService _addressService;
    private readonly NetworkDeviceService _networkDeviceService;
    private readonly IMessageService _messageService;
    private readonly int _addressId;

    public ObservableCollection<MountingPoint> MountingPoints { get; } = new();
    public ObservableCollection<NetworkDevice> FreeDevices { get; } = new();

    private int _selectedMountingPointId;
    public int SelectedMountingPointId
    {
        get => _selectedMountingPointId;
        set { _selectedMountingPointId = value; OnPropertyChanged(); }
    }

    private int? _selectedDeviceId;
    public int? SelectedDeviceId
    {
        get => _selectedDeviceId;
        set { _selectedDeviceId = value; OnPropertyChanged(); }
    }

    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => _errorMessage;
        set { _errorMessage = value; OnPropertyChanged(); }
    }

    public RelayCommand BindCommand { get; }
    public RelayCommand CancelCommand { get; }

    public Action? GoBack { get; set; }

    public BindDeviceViewModel(int addressId)
    {
        _addressService = new AddressService();
        _networkDeviceService = new NetworkDeviceService();
        _messageService = new MessageService();
        _addressId = addressId;

        BindCommand = new RelayCommand(_ => Bind());
        CancelCommand = new RelayCommand(_ => GoBack?.Invoke());

        LoadData();
    }

    private void LoadData()
    {
        var mp = _networkDeviceService
            .GetMountingPointsByAddressId(_addressId);
        MountingPoints.Clear();
        foreach (var m in mp)
        {
            MountingPoints.Add(m);
        }

        var freeDevices = _networkDeviceService
            .GetDevicesWithoutMountingPoint();
        FreeDevices.Clear();
        foreach (var d in freeDevices)
        {
            FreeDevices.Add(d);
        }

        if (MountingPoints.Count > 0)
        {
            SelectedMountingPointId = MountingPoints[0].Id;
        }
    }

    private void Bind()
    {
        if (SelectedMountingPointId <= 0 || !SelectedDeviceId.HasValue)
        {
            _messageService.ShowError("Выберите точку монтажа и устройство.");
            return;
        }

        try
        {
            _networkDeviceService
                .BindDeviceToMountingPoint(SelectedDeviceId.Value, SelectedMountingPointId);
            GoBack?.Invoke();
        }
        catch (Exception ex)
        {
            _messageService.ShowError(ex.Message);
        }
    }
}
