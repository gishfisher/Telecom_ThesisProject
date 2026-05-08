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

        public Connection? Connection { get; set; }

        public string PageTitle { get; private set; } = "Подключение";

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

        #region Observable Collections

        public ObservableCollection<Apartment> Apartments { get; } = new();
        public ObservableCollection<Address> Addresses { get; } = new();
        public ObservableCollection<Street> Streets { get; } = new();
        public ObservableCollection<City> Cities { get; } = new();
        public ObservableCollection<Client> Clients { get; } = new();
        public ObservableCollection<NetworkDevice> Devices { get; } = new();
        public ObservableCollection<DevicePortDto> DevicePorts { get; } = new();
        public ObservableCollection<DevicePortDto> AvailableDevicePorts { get; } = new();
        public ObservableCollection<Tariff> Tariffs { get; } = new();

        #endregion

        #region Relay Commands

        public RelayCommand? SaveCommand { get; }
        public RelayCommand? CancelCommand { get; }
        //public RelayCommand? AddClientCommand { get; }
        //public RelayCommand? OpenAddressesCommand { get; }

        #endregion

        #region Actions

        public Action? NavigateToAdresses { get; set; }
        public Action? GoBack { get; set; }
        public Action? NavigateToAddClient { get; set; }

        #endregion

        public ConnectionAddEditViewModel(Connection connection, int? targetPortId = null)
        {
            // Инициализация сервисов
            _addressService = new AddressService();
            _clientService = new ClientService();
            _networkDeviceService = new NetworkDeviceService();
            _connectionService = new ConnectionService();
            _tariffService = new TariffService();
            _messageService = new MessageService();

            // Построение заголовка страницы в зависимости от наличия данных о подключении
            PageTitle = BuildPageTitle(connection ?? new Connection());

            // Если передано существующее подключение, создаем его копию для редактирования, иначе инициализируем новое подключение
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

            // Если создается новое подключение и передан ID порта, пытаемся предзаполнить данные на основе этого порта
            if (Connection.Id == 0 && targetPortId.HasValue)
            {
                var port = _networkDeviceService.GetDevicePortById(targetPortId.Value);

                if (port == null) return;

                var device = _networkDeviceService.GetDeviceById(port.DeviceId);

                if (device?.MountingPoint?.Address != null)
                {
                    SelectedCityId = device.MountingPoint.Address.Street.CityId;
                    SelectedStreetId = device.MountingPoint.Address.StreetId;
                    SelectedAddressId = device.MountingPoint.AddressId;
                }

                SelectedDeviceId = port.DeviceId;
                SelectedDevicePortId = targetPortId.Value;
            }

            LoadData(); // Вызов метода загрузки данных для заполнения коллекций

            // Если редактируется существующее подключение, предзаполняем данные на основе его текущих значений
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
            //AddClientCommand = new RelayCommand(_ => NavigateToAddClient?.Invoke());
            //OpenAddressesCommand = new RelayCommand(_ => NavigateToAdresses?.Invoke());
        }

        // Метод сохранения данных

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

                if (Connection?.Id == 0)
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
                        Id = Connection?.Id ?? 0,
                        ApartmentId = apartmentId,
                        ClientId = SelectedClientId ?? 0,
                        TariffId = Connection?.TariffId ?? 0,
                        StaticIp = Connection?.StaticIp ?? "0.0.0.0",
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
      
        // === Метод загрузки данных ===

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

        // === Вспомогательные методы ===

        private static string BuildPageTitle(Connection connection)
        {
            if (connection.Id == 0)
                return "Создание подключения";

            var created = connection.CreatedAt;
            var datePart = created.HasValue
                ? created.Value.ToString("dd.MM.yyyy HH:mm")
                : "—";

            return $"Подключение №{connection.Id:D4} от {datePart}";
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

        private Connection CreateEditableObject(Connection connection)
        {
            if (connection == null)
            {
                return new Connection
                {
                    CreatedAt = DateTime.Now,
                };
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
                Tariff = connection.Tariff,
                CreatedAt = connection.CreatedAt,
            };
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
    }
}
