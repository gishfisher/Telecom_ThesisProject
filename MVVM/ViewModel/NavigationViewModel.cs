using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telecom.Utilities;
using Telecom_ThesisProject.Core;
using Telecom_ThesisProject.MVVM.Model;
using Telecom_ThesisProject.Services;

namespace Telecom_ThesisProject.MVVM.ViewModel
{
    class NavigationViewModel : ObservableObject
    {
        private object? _currentView;
        public object? CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        public HomeViewModel HomeViewModel { get; }
        public LoginViewModel LoginViewModel { get; }
        public ClientViewModel ClientViewModel { get; }

        public RelayCommand HomeViewCommand { get; }
        public RelayCommand ClientViewCommand { get; }
        public RelayCommand LogoutCommand { get; }

        public NavigationViewModel()
        {
            HomeViewModel = new HomeViewModel();
            ClientViewModel = new ClientViewModel();
            LoginViewModel = new LoginViewModel();
           
            LoginViewModel.OnLoginSuccess = user =>
            {
                CurrentSession.Login(user);
                CurrentView = HomeViewModel;
            };

            ClientViewCommand = new RelayCommand(
                o => CurrentView = ClientViewModel,
                o => IsAuthenticated);

            ClientViewModel.Navigate = client =>
            {
                var vm = new ClientAddEditViewModel(client);
                vm.GoBack = () =>
                {
                    ClientViewModel.Refresh();
                    CurrentView = ClientViewModel;
                };
                CurrentView = vm;
            };

            HomeViewCommand = new RelayCommand(
                o => CurrentView = HomeViewModel,
                o => IsAuthenticated);

            LogoutCommand = new RelayCommand(
                o => CurrentSession.Logout(),
                o => IsAuthenticated);

            CurrentSession.SessionChanged += OnSessionChanged;

            CurrentView = LoginViewModel;
        }

        private void OnSessionChanged()
        {
            OnPropertyChanged(nameof(IsAuthenticated));
            OnPropertyChanged(nameof(CanSeeNetworkMenu));
            OnPropertyChanged(nameof(CanSeeAdminMenu));

            if (IsAuthenticated)
                CurrentView = HomeViewModel;
            else
                CurrentView = LoginViewModel;
        }

        public bool IsAuthenticated => CurrentSession.IsAuthenticated;
        public bool CanSeeNetworkMenu => CurrentSession.CanSeeNetworkMenu;
        public bool CanSeeAdminMenu => CurrentSession.CanSeeAdminMenu;

    }
}