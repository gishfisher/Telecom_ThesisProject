using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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

        public Connection Connection { get; private set; }

        public string PageTitle { get; private set; } = "Подключение";

        // Аналог IsNewRequest / IsExistingRequest из RequestAddEditViewModel
        public bool IsNewConnection => Connection.Id == 0;
        public bool IsExistingConnection => Connection.Id != 0;

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

                if (value.HasValue)
                    CheckAddressHaveDevices(value.Value);
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

        private int? _selectedDeviceId;
        public int? SelectedDeviceId
        {
            get => _selectedDeviceId;
            set
            {
                _selectedDeviceId = value;
                OnPropertyChanged();

                SelectedDevicePortId = null;
                DevicePorts.Clear();

                if (value.HasValue)
                    LoadPorts();
            }
        }

        private int? _selectedDevicePortId;
        public int? SelectedDevicePortId
        {
            get => _selectedDevicePortId;
            set
            {
                _selectedDevicePortId = value;
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
        public ObservableCollection<Tariff> Tariffs { get; } = new();

        #endregion

        #region Relay Commands

        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }

        #endregion

        #region Actions

        public Action? GoBack { get; set; }
        public Action? NavigateToAddClient { get; set; }
        public Action? NavigateToAddresses { get; set; }

        #endregion

        public ConnectionAddEditViewModel(Connection? connection, int? targetPortId = null)
        {
            _addressService = new AddressService();
            _clientService = new ClientService();
            _networkDeviceService = new NetworkDeviceService();
            _connectionService = new ConnectionService();
            _tariffService = new TariffService();
            _messageService = new MessageService();

            // Создаём редактируемую копию объекта — аналог CreateEditableRequest
            Connection = CreateEditableConnection(connection, targetPortId);

            PageTitle = BuildPageTitle(Connection);
            OnPropertyChanged(nameof(PageTitle));

            // Загружаем справочники
            LoadData();

            // Предзаполняем поля если создаём по порту
            if (IsNewConnection && targetPortId.HasValue)
                PreFillFromPort(targetPortId.Value);

            // Предзаполняем поля если редактируем существующее подключение
            if (IsExistingConnection)
                PreFillFromConnection();

            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => GoBack?.Invoke());
        }

        // === Сохранение ===

        private void Save()
        {
            if (!Validate())
            {
                _messageService.ShowError(ErrorMessage);
                return;
            }

            try
            {
                int apartmentId = ResolveApartmentId();

                if (IsNewConnection)
                {
                    var newConnection = new Connection
                    {
                        ApartmentId = apartmentId,
                        ClientId = SelectedClientId!.Value,
                        TariffId = Connection.TariffId,
                        StaticIp = Connection.StaticIp,
                        PortId = SelectedDevicePortId!.Value,
                        CreatedAt = DateTime.Now
                    };

                    //var existingDevicePort = _networkDeviceService.GetDevicePortById(newConnection.PortId);
                    //var hasMountingPoint = existingDevicePort?.Device.MountingPoint != null;
                    //if (!hasMountingPoint)
                    //{
                    //    _messageService.ShowError("Прежде чем добавлять подключениие необходимо смонтировать оборудование");
                    //    return;
                    //}

                    _connectionService.AddConnection(newConnection);
                    _messageService.Show($"Новое подключение успешно добавлено!");
                }
                else
                {
                    var existing = _connectionService.GetConnectionById(Connection.Id);

                    var editedConnection = new Connection
                    {
                        Id = existing.Id,
                        ApartmentId = apartmentId,
                        ClientId = SelectedClientId ?? existing.ClientId,
                        TariffId = Connection.TariffId != 0 ? Connection.TariffId : existing.TariffId,
                        StaticIp = Connection.StaticIp ?? existing.StaticIp,
                        PortId = SelectedDevicePortId ?? existing.PortId,
                        CreatedAt = existing.CreatedAt
                    };

                    _connectionService.EditConnection(editedConnection);
                    _messageService.Show($"Подключение успешно отредактировано!");
                }
                GoBack?.Invoke();
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex.Message);
            }
        }

        // === Загрузка данных ===

        private void LoadData()
        {
            Clients.Clear();
            foreach (var client in _clientService.GetAll())
                Clients.Add(client);

            Cities.Clear();
            foreach (var city in _addressService.GetAllCities())
                Cities.Add(city);

            Tariffs.Clear();
            foreach (var tariff in _tariffService.GetAll())
                Tariffs.Add(tariff);
        }

        private void LoadStreets()
        {
            Streets.Clear();
            Addresses.Clear();
            Apartments.Clear();

            if (SelectedCityId == null) return;

            foreach (var street in _addressService.GetStreetsByCityId(SelectedCityId.Value))
                Streets.Add(street);
        }

        private void LoadAddresses()
        {
            Addresses.Clear();
            Apartments.Clear();

            if (SelectedStreetId == null) return;

            foreach (var addr in _addressService.GetAddressesByStreetId(SelectedStreetId.Value))
                Addresses.Add(addr);
        }

        private void LoadApartments()
        {
            Apartments.Clear();

            if (SelectedAddressId == null) return;

            foreach (var ap in _addressService.GetApartmentsByAddressId(SelectedAddressId.Value))
                Apartments.Add(ap);
        }

        private void LoadPorts()
        {
            if (SelectedDeviceId == null) return;

            var ports = _networkDeviceService.GetDevicePortsWithConnections(SelectedDeviceId.Value);
            if (ports == null) return;

            DevicePorts.Clear();
            foreach (var port in ports)
            {
                DevicePorts.Add(new DevicePortDto
                {
                    Id = port?.Id ?? 0,
                    PortName = port?.PortName ?? string.Empty,
                    HasConnection = port?.Connection != null || port?.IsUplink == true,
                    Connection = port?.Connection
                });
            }
        }

        private void CheckAddressHaveDevices(int addressId)
        {
            Devices.Clear();
            var devices = _networkDeviceService.GetDevicesByAddressId(addressId);
            if (devices.Count == 0)
            {
                _messageService.ShowError("Оборудования привязанного к адресу не найдено!");
                return;
            }

            foreach (var device in devices)
                Devices.Add(device);
        }

        // === Вспомогательные методы ===

        // Предзаполнение при создании подключения с конкретного порта
        private void PreFillFromPort(int portId)
        {
            var port = _networkDeviceService.GetDevicePortById(portId);
            if (port == null) return;

            var device = _networkDeviceService.GetDeviceById(port.DeviceId);

            if (device?.MountingPoint?.Address != null)
            {
                SelectedCityId = device.MountingPoint.Address.Street.CityId;
                SelectedStreetId = device.MountingPoint.Address.StreetId;
                SelectedAddressId = device.MountingPoint.AddressId;
            }

            SelectedDeviceId = port.DeviceId;
            SelectedDevicePortId = portId;
        }

        // Предзаполнение при редактировании существующего подключения
        private void PreFillFromConnection()
        {
            SelectedCityId = Connection.Apartment?.Address?.Street?.CityId;
            SelectedStreetId = Connection.Apartment?.Address?.StreetId;
            SelectedAddressId = Connection.Apartment?.AddressId;
            SelectedApartmentId = Connection.Apartment?.Id;
            SelectedClientId = Connection.ClientId;
            SelectedDeviceId = Connection.Port?.DeviceId;
            SelectedDevicePortId = Connection.PortId;
        }

        // Получение или создание квартиры
        private int ResolveApartmentId()
        {
            int addressId = SelectedAddressId!.Value;

            if (SelectedApartmentId.HasValue)
                return SelectedApartmentId.Value;

            var exists = _addressService.ApartamentIsExist(addressId, ApartmentNumber);
            if (!exists)
            {
                _addressService.AddApartment(new Apartment
                {
                    AddressId = addressId,
                    Number = ApartmentNumber
                });
            }

            return _addressService
                .GetApartmentIdByAddressIdAndNumber(addressId, ApartmentNumber).Id;
        }

        // Построение заголовка страницы
        private static string BuildPageTitle(Connection connection)
        {
            if (connection.Id == 0)
                return "Создание подключения";

            var datePart = connection.CreatedAt.HasValue
                ? connection.CreatedAt.Value.ToString("dd.MM.yyyy HH:mm")
                : "—";

            return $"Подключение №{connection.Id:D4} от {datePart}";
        }

        // Создание копии редактирумеого объекта
        private static Connection CreateEditableConnection(Connection? connection, int? targetPortId)
        {
            if (connection == null)
            {
                return new Connection
                {
                    PortId = targetPortId ?? 0,
                    CreatedAt = DateTime.Now
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
                CreatedAt = connection.CreatedAt,
                Port = connection.Port,
                Apartment = connection.Apartment,
                Client = connection.Client,
                Tariff = connection.Tariff,
            };
        }

        // Проверка ошибок
        private bool Validate()
        {
            if (Connection == null)
            {
                ErrorMessage = "Ошибка данных подключения.";
                return false;
            }

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
                errors.AppendLine("Квартира не выбрана и номер не указан.");
            if (SelectedDeviceId == null)
                errors.AppendLine("Устройство не выбрано.");
            if (SelectedDevicePortId == null)
                errors.AppendLine("Порт не выбран.");

            if (errors.Length > 0)
            {
                ErrorMessage = errors.ToString().Trim();
                return false;
            }

            ErrorMessage = string.Empty;
            return true;
        }

        public void Refresh()
        {
            LoadData();
        }
    }
}