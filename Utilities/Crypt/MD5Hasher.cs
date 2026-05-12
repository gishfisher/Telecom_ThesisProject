using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TelecomCompany.ApplicationData.Crypt
{
    public class MD5Hasher
    {
        public static string HashPassword(string password)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));
            using var md5 = MD5.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(password);
            byte[] hash = md5.ComputeHash(bytes);

            var sb = new StringBuilder(hash.Length * 2);
            foreach (var b in hash) sb.Append(b.ToString("X2"));
            return sb.ToString();
        }
    }
}
