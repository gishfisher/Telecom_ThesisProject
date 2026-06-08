using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Telecom.Utilities;
using Telecom_ThesisProject.Core;
using Telecom_ThesisProject.Data;
using Telecom_ThesisProject.MVVM.Model;
using Telecom_ThesisProject.Services;

namespace Telecom_ThesisProject.MVVM.ViewModel
{
    class HomeViewModel : ObservableObject
    {
        private RequestService _requestService;
        private ConnectionService _connectionService;
        private NetworkDeviceService _deviceService;

        public HomeViewModel()
        {
            _requestService = new RequestService();
            _connectionService = new ConnectionService();
            _deviceService = new NetworkDeviceService();

            LoadData();
        }

        // ===== Пользователь =====

        public string WelcomeMessage =>
            $"Добро пожаловать, {CurrentSession.CurrentUser?.Employee?.FirstName ?? "Пользователь"}!";

        public string CurrentUserRole =>
            MapRoles(CurrentSession.CurrentUser?.Role?.Id ?? 0);

        public string CurrentDateTime =>
            DateTime.Now.ToString("dd MMMM yyyy");

        public string CurrentTime =>
            DateTime.Now.ToString("HH:mm");

        // === Счётчики ===

        private int _connectionsCount;
        public int ConnectionsCount
        {
            get => _connectionsCount;
            set { _connectionsCount = value; OnPropertyChanged(); }
        }

        private int _devicesCount;
        public int DevicesCount
        {
            get => _devicesCount;
            set { _devicesCount = value; OnPropertyChanged(); }
        }

        private int _requestsCount;
        public int RequestsCount
        {
            get => _requestsCount;
            set { _requestsCount = value; OnPropertyChanged(); }
        }

        private int _activeRequestsCount;
        public int ActiveRequestsCount
        {
            get => _activeRequestsCount;
            set { _activeRequestsCount = value; OnPropertyChanged(); }
        }

        // === Загрузка данных ===

        private void LoadData()
        {
            // Счётчики
            DevicesCount = _deviceService.GetDevicesCount();
            ConnectionsCount = _connectionService.GetConnetctionsCount();
            RequestsCount = _requestService.GetRequestsCount();

            // Заявки по статусам
            ActiveRequestsCount = _requestService
                .GetActiveRequestsCount();
        }

        private string MapRoles(int roleId)
        {
            switch (roleId)
            {
                case 1:
                    return "Администратор";
                case 2:
                    return "Менеджер";
                case 3:
                    return "Системный администратор";
                default:
                    return "Пользователь";
            }
        }

        public void Refresh()
        {
            LoadData();
        }
    }
}