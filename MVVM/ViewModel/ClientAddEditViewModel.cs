using Shumakov_Telecom.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telecom_ThesisProject.Core;
using Telecom_ThesisProject.MVVM.Model;

namespace Telecom_ThesisProject.MVVM.ViewModel
{
    class ClientAddEditViewModel : ObservableObject
    {
        private readonly ClientService _clientService;

        public Client Client { get; set; }

        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }

        public Action GoBack { get; set; }

        public ClientAddEditViewModel(Client selectedClient)
        {
            Client = selectedClient ?? new Client();

            _clientService = new ClientService();

            SaveCommand = new RelayCommand(o => Save());
            CancelCommand = new RelayCommand(o => GoBack?.Invoke());
        }

        private void Save()
        {
            if (Client.Id == 0)
                _clientService.AddClient(Client);
            else
                _clientService.EditClient(Client);

            GoBack?.Invoke();
        }
    }
}
