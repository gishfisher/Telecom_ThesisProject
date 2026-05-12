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
    class EmployeeViewModel : ObservableObject
    {
        private readonly EmployeeService _employeeService;
        private readonly IMessageService _messageService;

        public ObservableCollection<Employee> Employee { get; set; }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterEmployees();
            }
        }

        private Employee _selectedEmployee;
        public Employee SelectedEmployee
        {
            get => _selectedEmployee;
            set { _selectedEmployee = value; OnPropertyChanged(); }
        }

        public RelayCommand AddCommand { get; }
        public RelayCommand EditCommand { get; }
        public RelayCommand DeleteCommand { get; }

        public Action<Employee> Navigate { get; set; }

        public EmployeeViewModel()
        {
            _employeeService = new EmployeeService();
            _messageService = new MessageService();
            
            Employee = new ObservableCollection<Employee>();

            LoadEmployees();
            FilterEmployees();

            AddCommand = new RelayCommand(
                o => Navigate?.Invoke(null!),
                o => !CurrentSession.IsSysAdmin);

            EditCommand = new RelayCommand(
                o => Navigate?.Invoke(SelectedEmployee),
                o => SelectedEmployee != null && !CurrentSession.IsSysAdmin);

            DeleteCommand = new RelayCommand(
                o => DeleteEmployees(),
                o => SelectedEmployee != null && !CurrentSession.IsSysAdmin);
        }

        private void LoadEmployees()
        {
            var list = _employeeService.GetAll();

            Employee.Clear();
            foreach (var e in list)
                Employee.Add(e);

            OnPropertyChanged(nameof(Employee));
        }

        private void FilterEmployees()
        {
            var search = SearchText?.Trim() ?? string.Empty;
            var all = _employeeService.GetAll();
            var filtered = string.IsNullOrEmpty(search)
                ? all
                : all.Where(x =>
                    (x.GetFullName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();

            Employee.Clear();
            foreach (var e in filtered)
                Employee.Add(e);
        }

        private void DeleteEmployees()
        {
            if (SelectedEmployee == null) return;

            if (_messageService.Confirm($"Удалить сотрудника {SelectedEmployee.GetFullName}?"))
            {
                try
                {
                    _employeeService.DeleteEmployee(SelectedEmployee);
                    LoadEmployees();
                    FilterEmployees();
                }
                catch (Exception ex) 
                {
                    _messageService.ShowError(ex.Message);
                }
            }    
        }
        public void Refresh()
        {
            LoadEmployees();
            FilterEmployees();
        }
    }
}
