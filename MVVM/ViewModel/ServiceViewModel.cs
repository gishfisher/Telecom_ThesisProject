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
    class ServiceViewModel : ObservableObject
    {
        private readonly ServiceService _serviceService;
        private readonly IMessageService _messageService;

        public ObservableCollection<Service> Service { get; set; }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterServices();
            }
        }

        private Service? _selectedService = null;
        public Service? SelectedService
        {
            get => _selectedService;
            set { _selectedService = value; OnPropertyChanged(); }
        }

        public RelayCommand AddCommand { get; }
        public RelayCommand EditCommand { get; }
        public RelayCommand DeleteCommand { get; }

        public Action<Service> Navigate { get; set; }

        public ServiceViewModel()
        {
            _serviceService = new ServiceService();
            _messageService = new MessageService();

            Service = new ObservableCollection<Service>();

            LoadServices();
            FilterServices();

            AddCommand = new RelayCommand(
                o => Navigate?.Invoke(null!),
                o => !CurrentSession.IsSysAdmin);

            EditCommand = new RelayCommand(
                o => Navigate?.Invoke(SelectedService),
                o => SelectedService != null && !CurrentSession.IsSysAdmin);

            DeleteCommand = new RelayCommand(
                o => DeleteService(),
                o => SelectedService != null && !CurrentSession.IsSysAdmin);
        }

        private void LoadServices()
        {
            var services = _serviceService.GetAll();

            Service.Clear();
            foreach (var service in services)
                Service.Add(service);

            OnPropertyChanged(nameof(Service));
        }

        private void FilterServices()
        {
            var search = SearchText?.Trim() ?? string.Empty;
            var all = _serviceService.GetAll();
            var filtered = string.IsNullOrEmpty(search)
                ? all
                : all.Where(x =>
                    (x.Name?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (x.Description?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();

            Service.Clear();
            foreach (var s in filtered)
                Service.Add(s);
        }

        private void DeleteService()
        {
            if (SelectedService == null) return;
            
            if (_messageService.Confirm($"Удалить услугу №{SelectedService.Id}, {SelectedService.Name}?"))
            {
                try
                {
                    _serviceService.RemoveService(SelectedService);
                    LoadServices();
                    FilterServices();

                }
                catch (Exception ex)
                {
                    _messageService.ShowError(ex.Message);
                }
            }
        }

        public void Refresh()
        {
            LoadServices();
            FilterServices();
        }
    }
}
