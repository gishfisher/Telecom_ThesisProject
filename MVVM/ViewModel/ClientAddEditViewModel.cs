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
        private readonly IMessageService _messageService;

        public Client Client { get; set; }

        public ObservableCollection<Address> Addresses { get; }

        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }

        public Action? GoBack { get; set; }

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

            Client = CreateEditableClient(selectedClient);

            Addresses = new ObservableCollection<Address>(_addressService.GetAllWithLocation());

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
                Client.Address = null!;

                if (Client.Id == 0)
                    _clientService.AddClient(Client);
                else
                    _clientService.EditClient(Client);

                GoBack?.Invoke();
            }
            catch (Exception ex)
            {
                _messageService.Show(ex.Message);
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

            if (string.IsNullOrWhiteSpace(Client.ContractNumber))
                errors.AppendLine("Номер договора обязателен.");
            else if (Client.ContractNumber.Length > 50)
                errors.AppendLine("Номер договора не должен превышать 50 символов.");

            if (Client.AddressId <= 0)
                errors.AppendLine("Выберите адрес.");

            if (errors.Length > 0)
            {
                ErrorMessage = errors.ToString().Trim();
                return false;
            }

            ErrorMessage = string.Empty;
            return true;
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
                ContractNumber = client.ContractNumber,
                AddressId = client.AddressId,
                Balance = client.Balance,
            };
        }
    }
}
