using System;
using System.Collections.Generic;
using System.Linq;
using Telecom_ThesisProject.Data;
using Telecom_ThesisProject.MVVM.Model;

namespace Telecom_ThesisProject.Services
{
    class ServiceService
    {
        public void AddService(Service service)
        {
            ArgumentNullException.ThrowIfNull(service);

            using (var db = new TelecomDbContext())
            {
                db.Services.Add(service);
                db.SaveChanges();
            }
        }

        public void EditService(Service service)
        {
            ArgumentNullException.ThrowIfNull(service);

            using (var db = new TelecomDbContext())
            {
                var existingService = db.Services
                    .FirstOrDefault(s => s.Id == service.Id) ?? throw new Exception("Услуга не найдена");

                existingService.Name = service.Name;
                existingService.Description = service.Description;

                db.SaveChanges();
            }
        }

        public void RemoveService(Service service)
        {
            ArgumentNullException.ThrowIfNull(service);

            using (var db = new TelecomDbContext())
            {
                var existingService = db.Services
                    .FirstOrDefault(s => s.Id == service.Id) ?? throw new Exception("Услуга не найдена");

                db.Services.Remove(existingService);
                db.SaveChanges();
            }
        }

        public List<Service> GetAll()
        {
            using (var db = new TelecomDbContext())
            {
                return db.Services.ToList();
            }
        }
    }
}
