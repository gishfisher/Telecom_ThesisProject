using System;
using System.Text;
using Telecom_ThesisProject.Core;
using Telecom_ThesisProject.MVVM.Model;
using Telecom_ThesisProject.Services;
using Telecom_ThesisProject.Utilities.Interfaces;

namespace Telecom_ThesisProject.MVVM.ViewModel
{
    class ServiceAddEditViewModel : ObservableObject
    {
        private readonly ServiceService _serviceService;
        private readonly IMessageService _messageService;

        public Service Service { get; set; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }

        public Action? GoBack { get; set; }

        public string Title => Service.Id == 0 ? "Добавление услуги" : $"Редактирование услуги №{Service.Id:D4}, {Service.Name}";

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

        public ServiceAddEditViewModel(Service? selectedService)
        {
            _messageService = new MessageService();
            _serviceService = new ServiceService();

            Service = CreateEditableService(selectedService);

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
                if (Service.Id == 0)
                    _serviceService.AddService(Service);
                else
                    _serviceService.EditService(Service);

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

            if (Service == null)
            {
                ErrorMessage = "Ошибка данных услуги.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Service.Name))
                errors.AppendLine("Наименование услуги обязательно.");
            else if (Service.Name.Length > 100)
                errors.AppendLine("Наименование услуги не должно превышать 100 символов.");

            if (!string.IsNullOrWhiteSpace(Service.Description) && Service.Description.Length > 150)
                errors.AppendLine("Описание не должно превышать 150 символов.");

            if (errors.Length > 0)
            {
                ErrorMessage = errors.ToString().Trim();
                return false;
            }

            ErrorMessage = string.Empty;
            return true;
        }

        private static Service CreateEditableService(Service? service)
        {
            if (service == null)
                return new Service { Name = string.Empty };

            return new Service
            {
                Id = service.Id,
                Name = service.Name,
                Description = service.Description,
            };
        }
    }
}
