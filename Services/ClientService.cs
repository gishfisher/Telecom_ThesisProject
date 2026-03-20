using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telecom_ThesisProject.Data;
using Telecom_ThesisProject.MVVM.Model;
using TelecomCompany.ApplicationData.Crypt;

namespace Shumakov_Telecom.Services
{
    internal class ClientService
    {
        private TelecomDbContext _db = new TelecomDbContext();

        public bool AddClient(Client client)
        {
            if (client == null) return false;

            _db.Clients.Add(client);
            _db.SaveChanges();

            return true;
        }

        public bool EditClient(Client client)
        {
            if (client == null) return false;

            _db.SaveChanges();

            return true;
        }

        public bool RemoveClient(Client client)
        {
            if (client == null) return false;

            _db.Clients.Remove(client);
            _db.SaveChanges();

            return true;
        }

        public List<Client> GetAll()
        {
            return _db.Clients.ToList();
        }
    }
}
