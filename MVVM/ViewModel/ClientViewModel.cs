using Shumakov_Telecom.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telecom.Utilities;
using Telecom_ThesisProject.Core;
using Telecom_ThesisProject.MVVM.Model;

namespace Telecom_ThesisProject.MVVM.ViewModel
{
    class ClientViewModel : ObservableObject
    {
        private readonly ClientService _clientService;

        public ObservableCollection<Client> Clients { get; set; }

        private string _searchText;
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

            LoadClients();

            AddCommand = new RelayCommand(
                o => Navigate?.Invoke(null!),
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
            Clients = new ObservableCollection<Client>(_clientService.GetAll());
            OnPropertyChanged(nameof(Clients));
        }

        public void Refresh()
        {
            LoadClients();
        }

        private void FilterClients()
        {
            var filtered = _clientService.GetAll()
                .Where(x => x.FullName.ToLower()
                .Contains(SearchText?.ToLower() ?? ""));

            Clients = new ObservableCollection<Client>(filtered);
            OnPropertyChanged(nameof(Clients));
        }

        private void DeleteClient()
        {
            _clientService.RemoveClient(SelectedClient);
            LoadClients();
        }
    }
}
