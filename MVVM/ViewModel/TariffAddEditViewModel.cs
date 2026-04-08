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
    class TariffAddEditViewModel : ObservableObject
    {
        private readonly TariffService _tariffService;
        private readonly ServiceService _serviceService;
        private readonly IMessageService _messageService;

        public Tariff Tariff { get; set; }
        public ObservableCollection<Service> AvailableServices { get; }
        public ObservableCollection<Service> SelectedServices { get; }

        private Service? _selectedServiceToAdd;
        public Service? SelectedServiceToAdd
        {
            get => _selectedServiceToAdd;
            set
            {
                _selectedServiceToAdd = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }
        public RelayCommand AddServiceCommand { get; }
        public RelayCommand RemoveServiceCommand { get; }

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

        public TariffAddEditViewModel(Tariff? selectedTariff)
        {
            _messageService = new MessageService();
            _tariffService = new TariffService();
            _serviceService = new ServiceService();

            Tariff = CreateEditableTariff(selectedTariff);

            AvailableServices = new ObservableCollection<Service>(_serviceService.GetAll());
            SelectedServices = new ObservableCollection<Service>(Tariff.Services);

            SaveCommand = new RelayCommand(o => Save());
            CancelCommand = new RelayCommand(o => GoBack?.Invoke());

            AddServiceCommand = new RelayCommand(
                o => AddSelectedService(),
                o => SelectedServiceToAdd != null);

            RemoveServiceCommand = new RelayCommand(
                o => RemoveService(o as Service),
                o => o is Service);
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
                Tariff.Services.Clear();
                foreach (var service in SelectedServices)
                    Tariff.Services.Add(service);

                if (Tariff.Id == 0)
                    _tariffService.AddTariff(Tariff);
                else
                    _tariffService.EditTariff(Tariff);

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

            if (Tariff == null)
            {
                ErrorMessage = "Ошибка данных тарифа.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Tariff.Name))
                errors.AppendLine("Наименование тарифа обязательно.");
            else if (Tariff.Name.Length > 100)
                errors.AppendLine("Наименование тарифа не должно превышать 100 символов.");

            if (Tariff.PricePerGb < 0)
                errors.AppendLine("Цена за ГБ не может быть отрицательной.");

            if (Tariff.MonthlyFee.HasValue && Tariff.MonthlyFee.Value < 0)
                errors.AppendLine("Абонентская плата не может быть отрицательной.");

            if (errors.Length > 0)
            {
                ErrorMessage = errors.ToString().Trim();
                return false;
            }

            ErrorMessage = string.Empty;
            return true;
        }

        private static Tariff CreateEditableTariff(Tariff? tariff)
        {
            if (tariff == null)
                return new Tariff { MonthlyFee = 0m, PricePerGb = 0m };

            var editable = new Tariff
            {
                Id = tariff.Id,
                Name = tariff.Name,
                PricePerGb = tariff.PricePerGb,
                MonthlyFee = tariff.MonthlyFee ?? 0m,
            };

            foreach (var service in tariff.Services)
                editable.Services.Add(service);

            return editable;
        }

        private void AddSelectedService()
        {
            if (SelectedServiceToAdd == null) return;
            if (!SelectedServices.Any(s => s.Id == SelectedServiceToAdd.Id))
                SelectedServices.Add(SelectedServiceToAdd);
        }

        private void RemoveService(Service? service)
        {
            if (service == null) return;
            if (SelectedServices.Contains(service))
                SelectedServices.Remove(service);
        }
    }
}
