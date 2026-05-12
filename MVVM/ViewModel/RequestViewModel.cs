using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telecom.Utilities;
using Telecom_ThesisProject.Core;
using Telecom_ThesisProject.MVVM.Model;
using Telecom_ThesisProject.Services;
using Telecom_ThesisProject.Utilities.Interfaces;

namespace Telecom_ThesisProject.MVVM.ViewModel
{
    class RequestViewModel : ObservableObject
    {
        private readonly RequestService _requestService;
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
                FilterRequests();
            }
        }

        private Request _selectedRequest = null;
        public Request SelectedRequest
        {
            get => _selectedRequest;
            set { _selectedRequest = value; OnPropertyChanged(); }
        }

        #endregion

        #region ObservableCollections
        
        public ObservableCollection<Request> Requests { get; set; }

        #endregion

        #region RelayCommands

        public RelayCommand AddCommand { get; }
        public RelayCommand EditCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand RequestDetailsCommand {  get; }

        #endregion

        #region Actions
        
        public Action<Request> Navigate { get; set; }
        public Action<Request> NavigateToDetails {  get; set; }

        #endregion

        public RequestViewModel()
        {
            _requestService = new RequestService();
            _messageService = new MessageService();

            Requests = new ObservableCollection<Request>();

            LoadRequests();
            FilterRequests();

            AddCommand = new RelayCommand(_ => Navigate?.Invoke(null!), _ => !CurrentSession.IsSysAdmin);
            EditCommand = new RelayCommand(_ => Navigate?.Invoke(SelectedRequest), _ => SelectedRequest != null && !CurrentSession.IsSysAdmin);
            DeleteCommand = new RelayCommand(_ => DeleteRequest(), _ => SelectedRequest != null && !CurrentSession.IsSysAdmin);
            RequestDetailsCommand = new RelayCommand(_ => NavigateToDetails.Invoke(SelectedRequest), _ => SelectedRequest != null);
        }

        private void LoadRequests()
        {
            var requests = _requestService.GetAll();

            Requests.Clear();
            foreach (var request in requests)
                Requests.Add(request);

            OnPropertyChanged(nameof(Requests));
        }

        private void FilterRequests()
        {
            var search = SearchText?.Trim() ?? string.Empty;
            var all = _requestService.GetAll();
            var filtered = string.IsNullOrEmpty(search)
                ? all
                : all.Where(x =>
                    (x.Client?.GetFullName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();

            Requests.Clear();
            foreach (var r in filtered)
                Requests.Add(r);
        }

        private void DeleteRequest()
        {
            if (SelectedRequest == null) return;

            if (_messageService.Confirm($"Удалить заявку №{SelectedRequest.Id} от {SelectedRequest.CreatedAt}?"))
            {
                try
                {
                    _requestService.RemoveRequest(SelectedRequest);
           
                    LoadRequests();
                    FilterRequests();
                }
                catch (Exception ex)
                {
                    _messageService.ShowError(ex.Message);
                }
            }
        }

        public void Refresh()
        {
            LoadRequests();
            FilterRequests();
        }
    }
}
