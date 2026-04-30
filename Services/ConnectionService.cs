using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telecom_ThesisProject.Data;
using Telecom_ThesisProject.MVVM.Model;

namespace Telecom_ThesisProject.Services
{
    public class ConnectionService
    {
        public void AddConnection(Connection connection)
        {
            ArgumentNullException.ThrowIfNull(connection);

            using (var db = new TelecomDbContext())
            {
                var existing = db.Connections.FirstOrDefault(c => c.ApartmentId == connection.ApartmentId);
                    
                if (existing != null) throw new Exception("Квартира уже подключена");

                db.Connections.Add(connection);
                db.SaveChanges();
            }
        }

        public void EditConnection(Connection connection)
        {
            ArgumentNullException.ThrowIfNull(connection);

            using (var db = new TelecomDbContext())
            {
                var existing = db.Connections.FirstOrDefault(c => c.Id == connection.Id)
                    ?? throw new Exception("Подключение не найдено");

                existing.StaticIp = connection.StaticIp;
                existing.TariffId = connection.TariffId;
                existing.ClientId = connection.ClientId;
                existing.ApartmentId = connection.ApartmentId;
                existing.PortId = connection.PortId;

                db.SaveChanges();
            }
        }

        public void DeleteConnection(Connection connection)
        {
            ArgumentNullException.ThrowIfNull(connection);

            using (var db = new TelecomDbContext())
            {
                db.Connections.Remove(connection);
                db.SaveChanges();
            }
        }

        public List<Connection> GetAllConnections()
        {
            using (var db = new TelecomDbContext())
            {
                return db.Connections
                    .AsNoTracking()
                    .AsQueryable()
                    .Include(c => c.Client)
                    .Include(c => c.Port)
                        .ThenInclude(c => c.Device)
                    .Include(c => c.Apartment)
                        .ThenInclude(c => c.Address)
                            .ThenInclude(c => c.Street)
                                .ThenInclude(c => c.City)
                    .Include(c => c.Tariff)
                    .ToList();
            }
        }

        public List<Connection> GetConnectionsByClientId(int clientId)
        {
            using (var db = new TelecomDbContext())
            {
                return db.Connections
                    .Where(c => c.ClientId == clientId)
                    .ToList();
            }
        }

        //public List<Connection> GetConnectionsByApartmentId(int apartmentId)
        //{
        //    using (var context = new TelecomDbContext())
        //    {
        //        return context.Connections
        //            .Where(c => c.ApartmentId == apartmentId)
        //            .ToList();
        //    }
        //}
    }
}
