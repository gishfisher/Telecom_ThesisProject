using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telecom_ThesisProject.Core;
using Telecom_ThesisProject.Data;
using Telecom_ThesisProject.MVVM.Model;
using Telecom_ThesisProject.Services;
using Telecom_ThesisProject.Utilities.Interfaces;

namespace Telecom_ThesisProject.MVVM.ViewModel
{
    class RequestAddEditViewModel : ObservableObject
    {
        private readonly RequestService _requestService;
        private readonly ClientService _clientService;
        private readonly EmployeeService _employeeService;
        private readonly IMessageService _messageService;

        public Request Request { get; set; }

        public ObservableCollection<Client> Clients { get; }
        public ObservableCollection<Employee> Employees { get; }
        public ObservableCollection<RequestStatus> Statuses { get; }

        public string PageTitle { get; private set; } = "Заявка";

        public bool IsNewRequest => Request.Id == 0;

        public bool IsExistingRequest => Request.Id != 0;

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

        public RequestAddEditViewModel(Request? selectedRequest)
        {
            _messageService = new MessageService();
            _requestService = new RequestService();
            _clientService = new ClientService();
            _employeeService = new EmployeeService();

            Request = CreateEditableRequest(selectedRequest);

            PageTitle = BuildPageTitle(Request);
            OnPropertyChanged(nameof(PageTitle));
            OnPropertyChanged(nameof(IsNewRequest));
            OnPropertyChanged(nameof(IsExistingRequest));

            Clients = new ObservableCollection<Client>(_clientService.GetAll());
            Employees = new ObservableCollection<Employee>(_employeeService.GetAll());
            Statuses = new ObservableCollection<RequestStatus>(GetAllStatuses());

            SaveCommand = new RelayCommand(o => Save());
            CancelCommand = new RelayCommand(o => GoBack?.Invoke());
        }

        private static string BuildPageTitle(Request request)
        {
            if (request.Id == 0)
                return "Новая заявка";

            var created = request.CreatedAt;
            var datePart = created.HasValue
                ? created.Value.ToString("dd.MM.yyyy HH:mm")
                : "—";

            return $"Заявка №{request.Id:D4} от {datePart}";
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
                DetachNavigationForPersistence();

                if (Request.Id == 0)
                    _requestService.AddRequest(Request);
                else
                    _requestService.EditRequest(Request);

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

            if (Request == null)
            {
                ErrorMessage = "Ошибка данных заявки.";
                return false;
            }

            if (Request.ClientId == null && Request.Client == null)
                errors.AppendLine("Укажите клиента.");

            if (!string.IsNullOrWhiteSpace(Request.Description) && Request.Description.Length > 300)
                errors.AppendLine("Описание слишком длинное (максимум 300 символов).");

            if (errors.Length > 0)
            {
                ErrorMessage = errors.ToString().Trim();
                return false;
            }

            ErrorMessage = string.Empty;
            return true;
        }

        private void DetachNavigationForPersistence()
        {
            Request.Client = null;
            Request.Employee = null;
            Request.Device = null;
            Request.Status = null;
        }

        private static Request CreateEditableRequest(Request? request)
        {
            if (request == null)
            {
                return new Request
                {
                    CreatedAt = DateTime.Now
                };
            }

            return new Request
            {
                Id = request.Id,
                Description = request.Description,
                CreatedAt = request.CreatedAt,
                StatusId = request.StatusId,
                ClientId = request.ClientId,
                DeviceId = request.DeviceId,
                EmployeeId = request.EmployeeId,
                Client = request.Client,
                Device = request.Device,
                Employee = request.Employee,
                Status = request.Status
            };
        }

        private static List<RequestStatus> GetAllStatuses()
        {
            using (var db = new TelecomDbContext())
            {
                return db.RequestStatuses.ToList();
            }
        }
    }
}
