using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Telecom_ThesisProject.Core;
using Telecom_ThesisProject.MVVM.Model;
using Telecom_ThesisProject.Services;
using Telecom_ThesisProject.Utilities.Interfaces;

namespace Telecom_ThesisProject.MVVM.ViewModel
{
    class ClientAddEditViewModel : ObservableObject
    {
        private readonly ClientService _clientService;
        private readonly AddressService _addressService;
        private readonly ConnectionService _connectionService;
        private readonly IMessageService _messageService;

        public Client Client { get; set; }

        public ObservableCollection<Address> Addresses { get; }
        public ObservableCollection<Connection> Connections { get; }

        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }

        public Action? GoBack { get; set; }

        public string PageTitle { get; private set; } = "Добавление клиента";

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        public ClientAddEditViewModel(Client? selectedClient)
        {
            _messageService = new MessageService();
            _clientService = new ClientService();
            _addressService = new AddressService();
            _connectionService = new ConnectionService();

            Client = CreateEditableClient(selectedClient);

            PageTitle = BuildPageTitle(Client);
            OnPropertyChanged(nameof(PageTitle));

            //LoadConnectionsData(Client);

            Addresses = new ObservableCollection<Address>(_addressService.GetAllWithLocation());
            Connections = new ObservableCollection<Connection>(_connectionService.GetConnectionsByClientId(Client.Id));

            SaveCommand = new RelayCommand(o => Save());
            CancelCommand = new RelayCommand(o => GoBack?.Invoke());
        }

        private void Save()
        {
            if (!Validate())
            {
                _messageService.Show(ErrorMessage);
                return;
            }

            try
            {
                if (Client.Id == 0)
                    _clientService.AddClient(Client);
                else
                    _clientService.EditClient(Client);

                GoBack?.Invoke();
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex.Message);
            }
        }

        private bool Validate()
        {
            var errors = new StringBuilder();

            if (Client == null)
            {
                ErrorMessage = "Ошибка данных клиента.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Client.LastName))
                errors.AppendLine("Фамилия обязательна.");
            else if (Client.LastName.Length > 50)
                errors.AppendLine("Фамилия не должна превышать 50 символов.");

            if (string.IsNullOrWhiteSpace(Client.FirstName))
                errors.AppendLine("Имя обязательно.");
            else if (Client.FirstName.Length > 50)
                errors.AppendLine("Имя не должно превышать 50 символов.");

            if (!string.IsNullOrWhiteSpace(Client.MiddleName) && Client.MiddleName.Length > 50)
                errors.AppendLine("Отчество не должно превышать 50 символов.");

            if (string.IsNullOrWhiteSpace(Client.PhoneNumber))
                errors.AppendLine("Номер телефона обязателен.");
            else if (Client.PhoneNumber?.Length > 20)
                errors.AppendLine("Номер телефона не должен превышать 20 символов.");

            if (errors.Length > 0)
            {
                ErrorMessage = errors.ToString().Trim();
                return false;
            }

            ErrorMessage = string.Empty;
            return true;
        }

        //private void LoadConnectionsData(Client client)
        //{
        //    if (client == null) return;

        //    var connections = _connectionService.GetConnectionsByClientId(client.Id).ToList();
        //    Connections.Clear();
        //    foreach (var connection in connections)
        //    {
        //        Connections.Add(connection);
        //    }
        //}

        private static string BuildPageTitle(Client client)
        {
            if (client.Id == 0)
                return "Добавление клиента";

            return $"Редактирование клиента №{client.Id:D4}, {client.GetFullNameIn}";
        }

        private static Client CreateEditableClient(Client? client)
        {
            if (client == null)
                return new Client();

            return new Client
            {
                Id = client.Id,
                LastName = client.LastName,
                FirstName = client.FirstName,
                MiddleName = client.MiddleName,
                PhoneNumber = client.PhoneNumber,
            };
        }
    }
}
