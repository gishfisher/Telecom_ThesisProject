using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Telecom_ThesisProject.Data;
using Telecom_ThesisProject.MVVM.Model;

namespace Telecom_ThesisProject.Services
{
    class ClientService
    {
        public void AddClient(Client client)
        {
            ArgumentNullException.ThrowIfNull(client);
            DetachNavigations(client);

            using (var db = new TelecomDbContext())
            {
                db.Clients.Add(client);
                db.SaveChanges();
            }
        }

        public void EditClient(Client client)
        {
            ArgumentNullException.ThrowIfNull(client);

            using (var db = new TelecomDbContext())
            {
                var existing = db.Clients
                    .FirstOrDefault(c => c.Id == client.Id) ?? throw new Exception("Клиент не найден");

                existing.FirstName = client.FirstName;
                existing.LastName = client.LastName;
                existing.MiddleName = client.MiddleName;
                existing.ContractNumber = client.ContractNumber;
                existing.AddressId = client.AddressId;
                existing.Balance = client.Balance;

                db.SaveChanges();
            }
        }

        public void RemoveClient(Client client)
        {
            ArgumentNullException.ThrowIfNull(client);

            using (var db = new TelecomDbContext())
            {
                var existing = db.Clients
                    .FirstOrDefault(c => c.Id == client.Id) ?? throw new Exception("Клиент не найден");

                db.Clients.Remove(existing);
                db.SaveChanges();
            }
        }

        public List<Client> GetAll()
        {
            using (var db = new TelecomDbContext())
            {
                return db.Clients
                    .Include(c => c.Address)
                    .ThenInclude(a => a.Street)
                    .ThenInclude(s => s.City)
                    .ToList();
            }
        }

        private static void DetachNavigations(Client client)
        {
            client.Address = null!;
        }
    }
}
