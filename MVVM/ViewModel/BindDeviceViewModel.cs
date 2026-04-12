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
        _messageService = new MessageService();
        _addressId = addressId;

        BindCommand = new RelayCommand(_ => Bind());
        CancelCommand = new RelayCommand(_ => GoBack?.Invoke());

        LoadData();
    }

    private void LoadData()
    {
        using var db = new TelecomDbContext();

        var addr = db.Addresses
            .Include(a => a.MountingPoints).ThenInclude(mp => mp.PointType)
            .FirstOrDefault(a => a.Id == _addressId);

        if (addr != null)
        {
            foreach (var mp in addr.MountingPoints)
            {
                db.Entry(mp).State = EntityState.Detached;
                if (mp.PointType != null) db.Entry(mp.PointType).State = EntityState.Detached;
                MountingPoints.Add(mp);
            }
        }

        var freeDevices = db.NetworkDevices
            .Include(d => d.DeviceType)
            .Where(d => d.MountingPointId == null)
            .OrderBy(d => d.Name)
            .ToList();

        foreach (var d in freeDevices)
        {
            db.Entry(d).State = EntityState.Detached;
            if (d.DeviceType != null) db.Entry(d.DeviceType).State = EntityState.Detached;
            FreeDevices.Add(d);
        }

        if (MountingPoints.Count > 0)
            SelectedMountingPointId = MountingPoints[0].Id;
    }

    private void Bind()
    {
        if (SelectedMountingPointId <= 0 || !SelectedDeviceId.HasValue)
        {
            _messageService.Show("Выберите точку монтажа и устройство.");
            return;
        }

        try
        {
            using var db = new TelecomDbContext();
            var device = db.NetworkDevices.FirstOrDefault(d => d.Id == SelectedDeviceId.Value);
            if (device == null)
            {
                _messageService.Show("Устройство не найдено.");
                return;
            }
            device.MountingPointId = SelectedMountingPointId;
            db.SaveChanges();
            GoBack?.Invoke();
        }
        catch (Exception ex)
        {
            _messageService.Show(ex.Message);
        }
    }
}
