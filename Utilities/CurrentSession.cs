using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telecom_ThesisProject.MVVM.Model;

namespace Telecom.Utilities
{
    public static class CurrentSession
    {
        private static User? _currentUser;

        public static event Action? SessionChanged;

        public static User? CurrentUser
        {
            get => _currentUser;
            private set
            {
                _currentUser = value;
                SessionChanged?.Invoke();
            }
        }

        public static bool IsAuthenticated => CurrentUser != null;
        public static string? CurrentRole => CurrentUser?.Role?.Name;
        public static bool IsAdmin => CurrentRole == "Administrator";
        public static bool IsManager => CurrentRole == "Manager";
        public static bool IsSysAdmin => CurrentRole == "SysAdmin";
        public static bool CanSeeNetworkMenu => IsAdmin || IsSysAdmin;
        public static bool CanSeeAdminMenu => IsAdmin == true;
        
        public static void Login(User user)
        {
            CurrentUser = user;
        }

        public static void Logout()
        {
            CurrentUser = null;
        }
    }
}
