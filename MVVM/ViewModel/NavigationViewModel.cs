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

        // ViewModels
        public HomeViewModel HomeViewModel { get; }
        public LoginViewModel LoginViewModel { get; }
        public ClientViewModel ClientViewModel { get; }
        public EmployeeViewModel EmployeeViewModel { get; }
        public RequestViewModel RequestViewModel { get; }
        public ServiceViewModel ServiceViewModel { get; }
        public TariffViewModel TariffViewModel { get; }
        public NetworkDeviceViewModel NetworkDeviceViewModel { get; }
        public ConnectionsViewModel ConnectionsViewModel { get; }
        public AddressTreeViewModel AddressTreeViewModel { get; }

        //  Commands
        public RelayCommand HomeViewCommand { get; }
        public RelayCommand ClientViewCommand { get; }
        public RelayCommand LogoutCommand { get; }
        public RelayCommand EmployeeViewCommand { get; }
        public RelayCommand RequestViewCommand { get; }
        public RelayCommand ServiceViewCommand { get; }
        public RelayCommand TariffViewCommand { get; }
        public RelayCommand NetworkDevicesViewCommand { get; }
        public RelayCommand ConnectionsViewCommand { get; }
        public RelayCommand AddressTreeViewCommand { get; }

        public NavigationViewModel()
        {
            HomeViewModel = new HomeViewModel();
            ClientViewModel = new ClientViewModel();
            LoginViewModel = new LoginViewModel();
            EmployeeViewModel = new EmployeeViewModel();
            ServiceViewModel = new ServiceViewModel();
            TariffViewModel = new TariffViewModel();
            RequestViewModel = new RequestViewModel();
            NetworkDeviceViewModel = new NetworkDeviceViewModel();
            ConnectionsViewModel = new ConnectionsViewModel();
            AddressTreeViewModel = new AddressTreeViewModel();

            // Views
            // Clients
            ClientViewCommand = new RelayCommand(
                o => CurrentView = ClientViewModel,
                o => IsAuthenticated);

            ClientViewModel.Navigate = client =>
            {
                var vm = new ClientAddEditViewModel(client)
                {
                    GoBack = () =>
                    {
                        ClientViewModel.Refresh();
                        CurrentView = ClientViewModel;
                    }
                };
                CurrentView = vm;
            };

            // Employees + Users
            EmployeeViewCommand = new RelayCommand(
                o => CurrentView = EmployeeViewModel,
                o => IsAuthenticated);

            EmployeeViewModel.Navigate = emp =>
            {
                var vm = new EmployeeAddEditViewModel(emp)
                {
                    GoBack = () =>
                    {
                        EmployeeViewModel.Refresh();
                        CurrentView = EmployeeViewModel;
                    }
                };
                CurrentView = vm;
            };

            // Requests
            RequestViewCommand = new RelayCommand(
                o => CurrentView = RequestViewModel,
                o => IsAuthenticated);

            RequestViewModel.Navigate = req =>
            {
                var vm = new RequestAddEditViewModel(req)
                {
                    GoBack = () =>
                    {
                        RequestViewModel.Refresh();
                        CurrentView = RequestViewModel;
                    }
                };
                CurrentView = vm;
            };

            // Services
            ServiceViewCommand = new RelayCommand(
                o => CurrentView = ServiceViewModel,
                o => IsAuthenticated);

            ServiceViewModel.Navigate = service =>
            {
                var vm = new ServiceAddEditViewModel(service)
                {
                    GoBack = () =>
                    {
                        ServiceViewModel.Refresh();
                        CurrentView = ServiceViewModel;
                    }
                };
                CurrentView = vm;
            };

            // Tariffs
            TariffViewCommand = new RelayCommand(
                o => CurrentView = TariffViewModel,
                o => IsAuthenticated);

            TariffViewModel.Navigate = tariff =>
            {
                var vm = new TariffAddEditViewModel(tariff)
                {
                    GoBack = () =>
                    {
                        TariffViewModel.Refresh();
                        CurrentView = TariffViewModel;
                    }
                };
                CurrentView = vm;
            };

            //Network Devices
            NetworkDevicesViewCommand = new RelayCommand(
                o => CurrentView = NetworkDeviceViewModel,
                o => IsAuthenticated && CurrentSession.CanSeeNetworkMenu);

            NetworkDeviceViewModel.NavigateEditDevice = device =>
            {
                var vm = new NetworkDeviceAddEditViewModel(device)
                {
                    GoBack = () =>
                    {
                        NetworkDeviceViewModel.Refresh();
                        CurrentView = NetworkDeviceViewModel;
                    }
                };
                CurrentView = vm;
            };

            NetworkDeviceViewModel.NavigateDeviceDetails = device =>
            {
                var vm = new NetworkDeviceDetailsViewModel(device)
                {
                    GoBack = () => CurrentView = NetworkDeviceViewModel
                };
                CurrentView = vm;
            };

            ConnectionsViewCommand = new RelayCommand(
                o => CurrentView = ConnectionsViewModel,
                o => IsAuthenticated);

            

            // Core
            LoginViewModel.OnLoginSuccess = user =>
            {
                CurrentSession.Login(user);
                CurrentView = HomeViewModel;
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

        // Check user privileges and update the UI accordingly
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