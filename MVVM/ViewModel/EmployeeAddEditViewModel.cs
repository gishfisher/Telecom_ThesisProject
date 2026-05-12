using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telecom_ThesisProject.Core;
using Telecom_ThesisProject.Data;
using Telecom_ThesisProject.MVVM.Model;
using Telecom_ThesisProject.Services;
using Telecom_ThesisProject.Utilities.Interfaces;
using Wpf.Ui.Controls;

namespace Telecom_ThesisProject.MVVM.ViewModel
{
    class EmployeeAddEditViewModel : ObservableObject
    {
        private readonly EmployeeService _employeeService;
        private readonly IMessageService _messageService;

        public string PlainPassword { get; set; }

        public Employee Employee { get; set; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }

        public ObservableCollection<Role> Roles { get; set; }

        public Action GoBack { get; set; }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        public EmployeeAddEditViewModel(Employee selectedEmployee)
        {
            _messageService = new MessageService();
            _employeeService = new EmployeeService();

            Roles = new ObservableCollection<Role>(_employeeService.GetAllRoles());

            Employee = CreateEditableEmployee(selectedEmployee);

            SaveCommand = new RelayCommand(ps => Save(ps));
            CancelCommand = new RelayCommand(o => GoBack?.Invoke());
        }

        private void Save(object parameter)
        {
            if (parameter is PasswordBox pb)
                PlainPassword = pb.Password;

            if (!Validate())
            {
                _messageService.Show(ErrorMessage);
                return;
            }

            try
            {
                if (Employee.Id == 0)
                {
                    _employeeService.RegisterEmployee(Employee, PlainPassword);
                }
                else
                {
                    _employeeService.EditEmployee(Employee, PlainPassword);
                }

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

            if (Employee == null)
            {
                ErrorMessage = "Ошибка данных сотрудника.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Employee.LastName))
                errors.AppendLine("Фамилия обязательна.");
            else if (Employee.LastName.Length > 50)
                errors.AppendLine("• Фамиилия не должна превышать 50 символов.");

            if (string.IsNullOrWhiteSpace(Employee.FirstName))
                errors.AppendLine("Имя обязательно.");
            else if (Employee.FirstName.Length > 50)
                errors.AppendLine("Имя не должно превышать 50 символов.");

            if (!string.IsNullOrWhiteSpace(Employee.MiddleName) && Employee.MiddleName?.Length > 50)
                errors.AppendLine("Отчество не должно превышать 50 символов.");

            if (Employee.User == null)
            {
                errors.AppendLine("Ошибка пользователя.");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(Employee.User.Login))
                    errors.AppendLine("Логин обязателен.");

                if (Employee.User.RoleId <= 0)
                    errors.AppendLine("Выберите роль.");

                if (Employee.Id == 0 && string.IsNullOrWhiteSpace(PlainPassword))
                    errors.AppendLine("Укажите пароль.");

                if (!string.IsNullOrWhiteSpace(PlainPassword) && PlainPassword.Length < 4)
                    errors.AppendLine("Пароль должен содержать минимум 4 символа.");
            }

            if (errors.Length > 0)
            {
                ErrorMessage = errors.ToString();
                return false;
            }

            ErrorMessage = string.Empty;
            return true;
        }

        private static Employee CreateEditableEmployee(Employee employee)
        {
            if (employee == null) return new Employee { User = new User() };

            return new Employee
            {
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                MiddleName = employee.MiddleName,

                User = new User
                {
                    Id = employee.User.Id,
                    Login = employee.User.Login,
                    RoleId = employee.User.RoleId,
                    IsActive = employee.User.IsActive
                }
            };
        }
    }
}
