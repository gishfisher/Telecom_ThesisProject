using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telecom_ThesisProject.Core;

namespace Telecom_ThesisProject.MVVM.ViewModel
{
    class NavigationViewModel : ObservableObject
    {
        public RelayCommand HomeViewCommand { get; set; }

        public HomeViewModel HomeViewModel { get; set; }

        private object _currentView;

        public object CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public NavigationViewModel()
        {
            HomeViewModel = new HomeViewModel();

            CurrentView = HomeViewModel;

            HomeViewCommand = new RelayCommand(o =>
            {
                CurrentView = HomeViewModel;
            });
        }
    }
}