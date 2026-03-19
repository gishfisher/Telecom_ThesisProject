using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telecom_ThesisProject.MVVM.Model;

namespace Telecom.Utilities
{
    using System;
    using Telecom_ThesisProject.MVVM.Model;

    namespace Telecom.Utilities
    {
        public static class UserSession
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

            public static string? UserName => CurrentUser?.Login;

            public static void Login(User user)
            {
                CurrentUser = user;
            }

            public static void Logout()
            {
                CurrentUser = null;
            }

            // Проверка роли
            public static bool IsInRole(string roleName)
            {
                return CurrentRole == roleName;
            }
        }
    }
}
