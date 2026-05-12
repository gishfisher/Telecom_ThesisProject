using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Telecom.Utilities;
using Telecom_ThesisProject.Core;
using Telecom_ThesisProject.Data;
using Telecom_ThesisProject.MVVM.Model;
using Telecom_ThesisProject.Services;
using Telecom_ThesisProject.Utilities.Interfaces;

namespace Telecom_ThesisProject.MVVM.ViewModel
{
    class ClientViewModel : ObservableObject
    {
        private readonly ClientService _clientService;
        private readonly IMessageService _messageService;

        public ObservableCollection<Client> Clients { get; set; }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterClients();
            }
        }

        private Client _selectedClient;
        public Client SelectedClient
        {
            get => _selectedClient;
            set { _selectedClient = value; OnPropertyChanged(); }
        }

        public RelayCommand AddCommand { get; }
        public RelayCommand EditCommand { get; }
        public RelayCommand DeleteCommand { get; }

        public Action<Client> Navigate { get; set; }

        public ClientViewModel()
        {
            _clientService = new ClientService();
            _messageService = new MessageService();

            Clients = new ObservableCollection<Client>();

            LoadClients();

            FilterClients();

            AddCommand = new RelayCommand(
                o => Navigate?.Invoke(null),
                o => !CurrentSession.IsSysAdmin);

            EditCommand = new RelayCommand(
                o => Navigate?.Invoke(SelectedClient),
                o => SelectedClient != null && !CurrentSession.IsSysAdmin);

            DeleteCommand = new RelayCommand(
                o => DeleteClient(),
                o => SelectedClient != null && !CurrentSession.IsSysAdmin);
        }

        private void LoadClients()
        {
            var clients = _clientService.GetAll();

            Clients.Clear();
            foreach (var client in clients)
                Clients.Add(client);

            OnPropertyChanged(nameof(Clients));
        }

        private void FilterClients()
        {
            var search = SearchText?.Trim() ?? string.Empty;
            var all = _clientService.GetAll();
            var filtered = string.IsNullOrEmpty(search)
                ? all
                : all.Where(x =>
                    (x.GetFullName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false));

            Clients.Clear();
            foreach (var c in filtered)
                Clients.Add(c);
        }

        private void DeleteClient()
        {
            if (SelectedClient == null) return;

            if (_messageService.Confirm($"Удалить клиента {SelectedClient.GetFullName}?"))
            {
                try
                {
                    _clientService.RemoveClient(SelectedClient);
                    LoadClients();
                    FilterClients();
                }
                catch (Exception ex) 
                {
                    _messageService.ShowError(ex.Message);
                }
            }
        }

        public void Refresh()
        {
            LoadClients();
            FilterClients();
        }
    }
}