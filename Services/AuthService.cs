using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telecom.Utilities;
using Telecom_ThesisProject.Data;
using Telecom_ThesisProject.MVVM.Model;
using Telecom_ThesisProject.MVVM.ViewModel;
using TelecomCompany.ApplicationData.Crypt;

namespace Telecom_ThesisProject.Services
{
    public class AuthService
    {
        public User? Authenticate(string login, string password)
        {
            string hashedPassword = MD5Hasher.HashPassword(password);

            using (var db = new TelecomDbContext())
            {
                return db.Users
                    .Include(e => e.Role)
                    .FirstOrDefault(e => e.Login == login && e.PasswordHash == hashedPassword);
            }
        }
    }
}
