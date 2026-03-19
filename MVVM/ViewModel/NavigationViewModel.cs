using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telecom.Utilities;
using Telecom.Utilities.Telecom.Utilities;
using Telecom_ThesisProject.Core;
using Telecom_ThesisProject.MVVM.Model;
using Telecom_ThesisProject.Services;

namespace Telecom_ThesisProject.MVVM.ViewModel
{
    class NavigationViewModel : ObservableObject
    {
        //private bool _isAuthenticated;
        //public bool IsAuthenticated
        //{
        //    get => _isAuthenticated;
        //    set
        //    {
        //        _isAuthenticated = value;
        //        OnPropertyChanged();
        //    }
        //}

        //private string _currentRole;
        //public string CurrentRole
        //{
        //    get => _currentRole;
        //    set { _currentRole = value; OnPropertyChanged(); }
        //}

        private object _currentView;
        public object CurrentView
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
                UserSession.Login(user);
            };

            HomeViewCommand = new RelayCommand(
                o => CurrentView = HomeViewModel,
                o => UserSession.IsAuthenticated);

            ClientViewCommand = new RelayCommand(
                o => CurrentView = ClientViewModel,
                o => UserSession.IsAuthenticated);

            LogoutCommand = new RelayCommand(
                o => UserSession.Logout(),
                o => UserSession.IsAuthenticated);


            UserSession.SessionChanged += OnSessionChanged;

            CurrentView = LoginViewModel;
        }

        private void OnSessionChanged()
        {
            OnPropertyChanged(nameof(IsAuthenticated));
            OnPropertyChanged(nameof(CurrentRole));

            if (UserSession.IsAuthenticated)
                CurrentView = HomeViewModel;
            else
                CurrentView = LoginViewModel;
        }

        public bool IsAuthenticated => UserSession.IsAuthenticated;
        public string? CurrentRole => UserSession.CurrentRole;


    }

    //class NavigationViewModel : ObservableObject
    //{
    //    private readonly NavigationService _navigationService;

    //    public HomeViewModel HomeViewModel { get; }
    //    public LoginViewModel LoginViewModel { get; }
    //    public ClientViewModel ClientViewModel { get; }

    //    public RelayCommand HomeViewCommand { get; set; }
    //    public RelayCommand ClientViewCommand { get; set; }


    //    private object _currentView;
    //    public object CurrentView
    //    {
    //        get => _currentView;
    //        set { _currentView = value; OnPropertyChanged(); }
    //    }

    //    public NavigationViewModel()
    //    {
    //        _navigationService = new NavigationService();
    //        _navigationService.NavigateAction = vm => CurrentView = vm;

    //        HomeViewModel = new HomeViewModel();
    //        ClientViewModel = new ClientViewModel();

    //        LoginViewModel = new LoginViewModel(_navigationService);

    //        LoginViewModel.OnLoginSuccess = () =>
    //        {
    //            CurrentView = HomeViewModel;
    //        };

    //        HomeViewCommand = new RelayCommand(o =>
    //            CurrentView = HomeViewModel);

    //        ClientViewCommand = new RelayCommand(o =>
    //            CurrentView = ClientViewModel);

    //        CurrentView = LoginViewModel;
    //    }

    //public NavigationViewModel()
    //{
    //    _navigationService = new NavigationService();
    //    _navigationService.NavigateAction = vm => CurrentView = vm;

    //    HomeViewModel = new HomeViewModel();
    //    LoginViewModel = new LoginViewModel(_navigationService);
    //    ClientViewModel = new ClientViewModel();

    //    HomeViewCommand = new RelayCommand(o =>
    //        _navigationService.Navigate(HomeViewModel));

    //    ClientViewCommand = new RelayCommand(o =>
    //        _navigationService.Navigate(ClientViewModel));

    //    CurrentView = LoginViewModel;
    //}

    //public RelayCommand HomeViewCommand { get; set; }
    //public RelayCommand LoginViewCommand { get; set; }

    //public HomeViewModel HomeViewModel { get; set; }
    //public LoginViewModel LoginViewModel { get; set; }

    //private object _currentView;

    //public object CurrentView
    //{
    //    get { return _currentView; }
    //    set
    //    {
    //        _currentView = value;
    //        OnPropertyChanged();
    //    }
    //}

    //public NavigationViewModel()
    //{
    //    HomeViewModel = new HomeViewModel();
    //    LoginViewModel = new LoginViewModel();

    //    CurrentView = LoginViewModel;

    //    HomeViewCommand = new RelayCommand(o =>
    //    {
    //        CurrentView = HomeViewModel;
    //    });

    //    LoginViewCommand = new RelayCommand(o =>
    //    {
    //        CurrentView = new LoginViewModel();
    //    });
    //}
    //}
}