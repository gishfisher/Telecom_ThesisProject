using System;
using System.Collections.ObjectModel;
using System.Linq;
using Telecom.Utilities;
using Telecom_ThesisProject.Core;
using Telecom_ThesisProject.MVVM.Model;
using Telecom_ThesisProject.Services;
using Telecom_ThesisProject.Utilities.Interfaces;

namespace Telecom_ThesisProject.MVVM.ViewModel;

class NetworkDeviceViewModel : ObservableObject
{
    private readonly NetworkDeviceService _networkDeviceService;
    private readonly IMessageService _messageService;

    #region Properties

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;
            OnPropertyChanged();
            FilterDevices();
        }
    }

    private NetworkDevice _selectedDevice;
    public NetworkDevice SelectedDevice
    {
        get => _selectedDevice;
        set 
        { 
            _selectedDevice = value;
            OnPropertyChanged(); 
        }
    }

    #endregion

    #region Observable Collections
    
    public ObservableCollection<NetworkDevice> Devices { get; }

    #endregion

    #region RelayCommands
    
    public RelayCommand AddCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand OpenDeviceDetailsCommand { get; }

    #endregion

    #region Actions

    public Action<NetworkDevice?>? NavigateAddEditDevice { get; set; }
    public Action<NetworkDevice?>? NavigateDeviceDetails { get; set; }
    //public Action GoBack { get; set; }

    #endregion

    public NetworkDeviceViewModel()
    {
        _networkDeviceService = new NetworkDeviceService();
        _messageService = new MessageService();
       
        Devices = new ObservableCollection<NetworkDevice>();

        LoadDevices();

        AddCommand = new RelayCommand(_ => NavigateAddEditDevice?.Invoke(null), _ => CurrentSession.CanSeeNetworkMenu);
        EditCommand = new RelayCommand(_ => NavigateAddEditDevice?.Invoke(SelectedDevice), _ => SelectedDevice != null && CurrentSession.CanSeeNetworkMenu);
        DeleteCommand = new RelayCommand(_ => DeleteDevice(), _ => SelectedDevice != null && CurrentSession.CanSeeNetworkMenu);
        OpenDeviceDetailsCommand = new RelayCommand(_ => NavigateDeviceDetails?.Invoke(SelectedDevice), _ => SelectedDevice != null);
    }

    private void DeleteDevice()
    {
        if (SelectedDevice == null) return;

        if (_messageService.Confirm($"Удалить устройство №{SelectedDevice.Id}, {SelectedDevice.Name} - {SelectedDevice.IpAddress}?"))
        {
            try
            {
                _networkDeviceService.RemoveDevice(SelectedDevice);
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex.Message);
                return;
            }

            Refresh();
        }
    }

    private void LoadDevices()
    {
        var list = _networkDeviceService.GetAllDevices();
        Devices.Clear();
        foreach (var d in list)
            Devices.Add(d);
        OnPropertyChanged(nameof(Devices));
    }

    private void FilterDevices()
    {
        var search = SearchText?.Trim() ?? string.Empty;
        var all = _networkDeviceService.GetAllDevices();
        var filtered = string.IsNullOrEmpty(search)
            ? all
            : all.Where(d =>
                (d.Name?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (d.IpAddress?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();

        Devices.Clear();
        foreach (var d in filtered)
            Devices.Add(d);
        OnPropertyChanged(nameof(Devices));
    }

    public void Refresh()
    {
        LoadDevices();
        FilterDevices();
    }
}
