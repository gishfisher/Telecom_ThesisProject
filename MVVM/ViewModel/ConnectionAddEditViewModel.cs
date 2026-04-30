using Azure.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Telecom_ThesisProject.Core;
using Telecom_ThesisProject.MVVM.Model;
using Telecom_ThesisProject.Services;
using Telecom_ThesisProject.Utilities.DTOs;
using Telecom_ThesisProject.Utilities.Interfaces;

namespace Telecom_ThesisProject.MVVM.ViewModel
{
    public class ConnectionAddEditViewModel : ObservableObject
    {
        private readonly AddressService _addressService;
        private readonly ClientService _clientService;
        private readonly NetworkDeviceService _networkDeviceService;
        private readonly ConnectionService _connectionService;
        private readonly TariffService _tariffService;
        private readonly IMessageService _messageService;

        #region Properties

        private string _apartmentNumber = string.Empty;
        public string ApartmentNumber
        {
            get => _apartmentNumber;
            set 
            { 
                _apartmentNumber = value; 
                OnPropertyChanged();
            }
        }

        private int? _selectedApartmentId;
        public int? SelectedApartmentId
        {
            get => _selectedApartmentId;
            set
            {
                _selectedApartmentId = value;
                OnPropertyChanged();

                if (value.HasValue && Apartments != null)
                {
                    var selected = Apartments.FirstOrDefault(a => a.Id == value.Value);
                    if (selected != null)
                    {
                        _apartmentNumber = selected.Number;
                        OnPropertyChanged(nameof(ApartmentNumber));
                    }
                }
            }
        }

        private int? _selectedAddressId;
        public int? SelectedAddressId
        {
            get => _selectedAddressId;
            set
            {
                _selectedAddressId = value;
                OnPropertyChanged();

                SelectedDeviceId = null;
                SelectedDevicePortId = null;
                Devices.Clear();

                LoadApartments();

                if (value != null)
                {
                    CheckAddressHaveDevices(value.Value);
                }
            }
        }

        private int? _selectedCityId;
        public int? SelectedCityId
        {
            get => _selectedCityId;
            set
            {
                _selectedCityId = value;
                OnPropertyChanged();
                LoadStreets();
            }
        }

        private int? _selectedStreetId;
        public int? SelectedStreetId
        {
            get => _selectedStreetId;
            set
            {
                _selectedStreetId = value;
                OnPropertyChanged();
                
                LoadAddresses();
            }
        }

        private int? _selectedClientId;
        public int? SelectedClientId
        {
            get => _selectedClientId;
            set 
            { 
                _selectedClientId = value; 
                OnPropertyChanged(); 
            }
        }

        private int? selectedDeviceId ;

        public int? SelectedDeviceId
        {
            get => selectedDeviceId ;
            set
            {
                selectedDeviceId = value;
                OnPropertyChanged();

                SelectedDevicePortId = null;
                DevicePorts.Clear();

                if (value != null)
                {
                    LoadPorts();
                }
            }
        }

        private int? _selectDevicePortId;

        public int? SelectedDevicePortId
        {
            get => _selectDevicePortId;
            set
            {
                _selectDevicePortId = value;
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
                OnPropertyChanged(); 
            }
        }

        #endregion

        public Connection Connection { get; set; }

        public ObservableCollection<Apartment> Apartments { get; } = new();
        public ObservableCollection<Address> Addresses { get; } = new();
        public ObservableCollection<Street> Streets { get; } = new();
        public ObservableCollection<City> Cities { get; } = new();
        public ObservableCollection<Client> Clients { get; } = new();
        public ObservableCollection<NetworkDevice> Devices { get; } = new();
        public ObservableCollection<DevicePortDto> DevicePorts { get; } = new();
        public ObservableCollection<DevicePortDto> AvailableDevicePorts { get; } = new();
        public ObservableCollection<Tariff> Tariffs { get; } = new();

        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }
        public RelayCommand AddClientCommand { get; }
        public RelayCommand OpenAddressesCommand { get; }

        public Action? GoBack { get; set; }
        public Action? NavigateToAdresses { get; set; }
        public Action? NavigateToAddClient { get; set; }

        public ConnectionAddEditViewModel(Connection? connection, int? targetPortId = null)
        {
            _addressService = new AddressService();
            _clientService = new ClientService();
            _networkDeviceService = new NetworkDeviceService();
            _connectionService = new ConnectionService();
            _tariffService = new TariffService();
            _messageService = new MessageService();

            if (connection != null)
            {
                Connection = CreateEditableObject(connection);
            }
            else
            {
                Connection = new Connection 
                { 
                    PortId = targetPortId ?? 0 
                };
            }

            if (Connection.Id == 0 && targetPortId.HasValue)
            {
                var port = _networkDeviceService.GetDevicePortById(targetPortId.Value);

                if (port == null) return;

                var device = _networkDeviceService.GetDeviceById(port.DeviceId);

                if (device?.MountingPoint.Address != null)
                {
                    SelectedCityId = device.MountingPoint.Address.Street.CityId;
                    SelectedStreetId = device.MountingPoint.Address.StreetId;
                    SelectedAddressId = device.MountingPoint.AddressId;
                }

                SelectedDeviceId = port.DeviceId;
                SelectedDevicePortId = targetPortId.Value;
            }

            LoadData();

            if (Connection.Id != 0)
            {
                SelectedCityId = Connection?.Apartment?.Address?.Street.CityId;
                SelectedStreetId = Connection?.Apartment?.Address?.StreetId;
                SelectedAddressId = Connection?.Apartment?.AddressId;
                SelectedApartmentId = Connection?.Apartment.Id;

                SelectedClientId = Connection?.ClientId;
                SelectedDeviceId = Connection?.Port?.DeviceId;
                SelectedDevicePortId = Connection?.PortId;
            }

            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => GoBack?.Invoke());
            AddClientCommand = new RelayCommand(_ => NavigateToAddClient?.Invoke());
            OpenAddressesCommand = new RelayCommand(_ => NavigateToAdresses?.Invoke());
        }

        public void Save()
        {
            if (!Validate()) return;

            try
            {
                int addressId = SelectedAddressId ?? 0;
                int apartmentId = SelectedApartmentId ?? 0;

                var apartmentExists = _addressService
                    .ApartamentIsExist(addressId, ApartmentNumber);
                
                if (!apartmentExists)
                {
                    _addressService.AddApartment(new Apartment
                    {
                        AddressId = addressId,
                        Number = ApartmentNumber
                    });

                    apartmentId = _addressService
                        .GetApartmentIdByAddressIdAndNumber(addressId, ApartmentNumber).Id;
                }

                if (Connection.Id == 0)
                {
                    var newConnection = new Connection
                    {
                        ApartmentId = apartmentId,
                        ClientId = SelectedClientId ?? 0,
                        TariffId = Connection.TariffId,
                        StaticIp = Connection.StaticIp,
                        PortId = SelectedDevicePortId ?? 0,
                    };
                    _connectionService.AddConnection(newConnection);
                }
                else
                {
                    var editConnection = new Connection
                    {
                        Id = Connection.Id,
                        ApartmentId = apartmentId,
                        ClientId = SelectedClientId ?? 0,
                        TariffId = Connection.TariffId,
                        StaticIp = Connection.StaticIp,
                        PortId = SelectedDevicePortId ?? 0,
                    };
                    _connectionService.EditConnection(editConnection);
                }

                LoadPorts();
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex.Message);
                return;
            }
        }

        public bool Validate()
        {
            var errors = new StringBuilder();
            if (SelectedClientId == null)
                errors.AppendLine("Клиент не выбран.");
            if (SelectedCityId == null)
                errors.AppendLine("Город не выбран.");
            if (SelectedStreetId == null)
                errors.AppendLine("Улица не выбрана.");
            if (SelectedAddressId == null)
                errors.AppendLine("Адрес не выбран.");
            if (SelectedApartmentId == null && string.IsNullOrWhiteSpace(ApartmentNumber))
                errors.AppendLine("Квартира не выбрана или не указан её номер.");
            if (SelectedDeviceId == null)
                errors.AppendLine("Устройство не выбрано.");
            if (SelectedDevicePortId == null)
                errors.AppendLine("Порт не выбран.");

            if (errors.Length > 0)
            {
                _messageService.ShowError(errors.ToString());
                return false;
            }

            return true;
        }

        public void Refresh()
        {
            LoadData();
        }

        private void LoadData()
        {
            var clients = _clientService.GetAll();

            Clients.Clear();
            foreach (var client in clients)
            {
                Clients.Add(client);
            }

            var cities = _addressService.GetAllCities();

            Cities.Clear();
            foreach (var city in cities)
            {
                Cities.Add(city); 
            }

            var tariffs = _tariffService.GetAll();

            Tariffs.Clear();
            foreach (var tariff in tariffs)
            {
                Tariffs.Add(tariff);
            }
        }

        private void LoadStreets()
        {
            Streets.Clear();
            Addresses.Clear();
            Apartments.Clear();

            if (SelectedCityId == null)
            {
                return;
            }

            var streets = _addressService.GetStreetsByCityId(SelectedCityId ?? 0);
            foreach (var street in streets) 
            {
                Streets.Add(street);
            }
        }

        private void LoadAddresses()
        {
            Addresses.Clear();
            Apartments.Clear();

            if (SelectedStreetId == null)
            {  
                return; 
            }

            var addresses = _addressService.GetAddressesByStreetId(SelectedStreetId ?? 0);
            foreach (var addr in addresses)
            {
                Addresses.Add(addr);
            }
        }

        private void LoadApartments()
        {
            Apartments.Clear();
            if (SelectedAddressId == null) 
            {
                return;
            }

            var apartments = _addressService.GetApartmentsByAddressId(SelectedAddressId.Value);
            foreach (var ap in apartments)
            {
                Apartments.Add(ap);
            }
        }

        private void LoadPorts()
        {
            if (SelectedDeviceId == null) return;

            var ports = _networkDeviceService.GetDevicePortsWithConnections(SelectedDeviceId ?? 0);

            if (ports == null) return;

            DevicePorts.Clear();
            foreach (var port in ports)
            {
                DevicePorts.Add(new DevicePortDto
                {
                    Id = port?.Id ?? 0,
                    PortName = port?.PortName ?? string.Empty,
                    HasConnection = port?.Connection != null ? true : port?.IsUplink == true ? true : false,
                    Connection = port?.Connection ?? null
                });
            }
        }

        private void CheckAddressHaveDevices(int addressId)
        {
            var haveDevices = _networkDeviceService.GetDevicesByAddressId(addressId);

            Devices.Clear();
            foreach (var device in haveDevices)
            {
                Devices.Add(device);
            }
        }

        private Connection CreateEditableObject(Connection connection)
        {
            if (connection == null)
            {
                return new Connection();
            }

            return new Connection
            {
                Id = connection.Id,
                ApartmentId = connection.ApartmentId,
                ClientId = connection.ClientId,
                PortId = connection.PortId,
                TariffId = connection.TariffId,
                StaticIp = connection.StaticIp,
                IsActive = connection.IsActive,
                Port = connection.Port,
                Apartment = connection.Apartment,
                Client = connection.Client,
                Requests = connection.Requests,
                Tariff = connection.Tariff,
            };
        }
    }
}
