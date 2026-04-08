using Telecom_ThesisProject.Services;
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

namespace Telecom_ThesisProject.MVVM.ViewModel
{
    class ClientViewModel : ObservableObject
    {
        private readonly ClientService _clientService;

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

        private Client? _selectedClient = null;
        public Client? SelectedClient
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
            Clients = new ObservableCollection<Client>();

            _clientService = new ClientService();

            LoadClients();

            FilterClients();

            AddCommand = new RelayCommand(
                o => Navigate.Invoke(null),
                o => !CurrentSession.IsSysAdmin);

            EditCommand = new RelayCommand(
                o => Navigate.Invoke(SelectedClient),
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
                    (x.FullName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (x.ContractNumber?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();

            Clients.Clear();
            foreach (var c in filtered)
                Clients.Add(c);
        }

        private void DeleteClient()
        {
            if (SelectedClient == null) return;
            _clientService.RemoveClient(SelectedClient);
            LoadClients();
            FilterClients();
        }
        public void Refresh()
        {
            LoadClients();
            FilterClients();
        }
    }
}