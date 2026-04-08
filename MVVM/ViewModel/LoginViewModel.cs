using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Telecom.Utilities;
using Telecom_ThesisProject.Core;
using Telecom_ThesisProject.MVVM.Model;
using Telecom_ThesisProject.Services;

namespace Telecom_ThesisProject.MVVM.ViewModel
{
    class LoginViewModel : ObservableObject
    {
        private readonly AuthService _authService;

        public Action<User> OnLoginSuccess { get; set; }

        public RelayCommand LoginCommand { get; }

        private string _login;
        private string _errorMessage;

        public string Login
        {
            get => _login;
            set { _login = value; OnPropertyChanged(); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public LoginViewModel()
        {
            _authService = new AuthService();
            LoginCommand = new RelayCommand(
                o => ExecuteLogin(o));
        }

        private void ExecuteLogin(object parameter)
        {
            var passwordBox = parameter as PasswordBox;
            string password = passwordBox?.Password;

            var user = _authService.Authenticate(Login, password);

            if (user != null)
            {
                OnLoginSuccess?.Invoke(user);
            }
            else
            {
                ErrorMessage = "Неверный логин или пароль!";
            }
        }
    }
}
