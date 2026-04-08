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

namespace Telecom_ThesisProject.MVVM.ViewModel
{
    class RequestViewModel : ObservableObject
    {
        private readonly RequestService _requestService;

        public ObservableCollection<Request> Requests { get; set; }

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

        private Request? _selectedRequest = null;
        public Request? SelectedRequest
        {
            get => _selectedRequest;
            set { _selectedRequest = value; OnPropertyChanged(); }
        }

        public RelayCommand AddCommand { get; }
        public RelayCommand EditCommand { get; }
        public RelayCommand DeleteCommand { get; }

        public Action<Request> Navigate { get; set; }

        public RequestViewModel()
        {
            Requests = new ObservableCollection<Request>();

            _requestService = new RequestService();

            LoadRequests();
            FilterRequests();

            AddCommand = new RelayCommand(
                o => Navigate?.Invoke(null!),
                o => !CurrentSession.IsSysAdmin);

            EditCommand = new RelayCommand(
                o => Navigate?.Invoke(SelectedRequest),
                o => SelectedRequest != null && !CurrentSession.IsSysAdmin);

            DeleteCommand = new RelayCommand(
                o => DeleteRequest(),
                o => SelectedRequest != null && !CurrentSession.IsSysAdmin);
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
                    (x.Title?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (x.Client?.FullName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();

            Requests.Clear();
            foreach (var r in filtered)
                Requests.Add(r);
        }

        private void DeleteRequest()
        {
            if (SelectedRequest == null) return;
            _requestService.RemoveRequest(SelectedRequest);
            LoadRequests();
            FilterRequests();
        }

        public void Refresh()
        {
            LoadRequests();
            FilterRequests();
        }
    }
}
