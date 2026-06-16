using System;
using System.Windows.Controls;
using Telecom.Utilities;
using Telecom_ThesisProject.Core;
using Telecom_ThesisProject.MVVM.Model;
using Telecom_ThesisProject.Services;
using Telecom_ThesisProject.Utilities.Interfaces;

namespace Telecom_ThesisProject.MVVM.ViewModel
{
    class LoginViewModel : ObservableObject
    {
        private readonly AuthService _authService;
        private readonly IMessageService _messageService;

        public Action<User>? OnLoginSuccess { get; set; }

        public RelayCommand LoginCommand { get; }

        private string? _login;
        private string? _errorMessage;

        public string? Login
        {
            get => _login;
            set { _login = value; OnPropertyChanged(); }
        }

        public string? ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public LoginViewModel()
        {
            _authService = new AuthService();
            _messageService = new MessageService();

            LoginCommand = new RelayCommand(o => ExecuteLogin(o));
        }

        private void ExecuteLogin(object? parameter)
        {
            string? password = null;

            if (parameter is PasswordBox pb) password = pb.Password;
            else if (parameter != null)
            {
                var prop = parameter.GetType().GetProperty("Password");
                password = prop?.GetValue(parameter) as string;
            }

            if (string.IsNullOrEmpty(password))
            {
                ErrorMessage = "Введите пароль";
                _messageService.ShowError(ErrorMessage);
                return;
            }

            var user = _authService.Authenticate(Login ?? string.Empty, password);

            if (user == null)
            {
                ErrorMessage = "Неверный логин или пароль!";
                _messageService.ShowError(ErrorMessage);
                return;
            }

            if (!user.IsActive)
            {
                ErrorMessage = "Авторизация недоступна. Обратитесь к администратору.";
                _messageService.ShowError(ErrorMessage);
                return;
            }

            OnLoginSuccess?.Invoke(user);
        }
    }
}