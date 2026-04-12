using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Telecom_ThesisProject.Data;
using Telecom_ThesisProject.MVVM.Model;

namespace Telecom_ThesisProject.Services
{
    class TariffService
    {
        public void AddTariff(Tariff tariff)
        {
            ArgumentNullException.ThrowIfNull(tariff);

            using (var db = new TelecomDbContext())
            {
                var serviceIds = tariff.Services.Select(s => s.Id).ToList();
                var services = db.Services
                    .Where(s => serviceIds.Contains(s.Id))
                    .ToList();

                tariff.Services.Clear();
                foreach (var service in services)
                    tariff.Services.Add(service);

                db.Tariffs.Add(tariff);
                db.SaveChanges();
            }
        }

        public void EditTariff(Tariff tariff)
        {
            ArgumentNullException.ThrowIfNull(tariff);

            using (var db = new TelecomDbContext())
            {
                var existingTariff = db.Tariffs
                    .Include(t => t.Services)
                    .FirstOrDefault(t => t.Id == tariff.Id) ?? throw new Exception("Тариф не найден");

                existingTariff.Name = tariff.Name;
                existingTariff.MonthlyFee = tariff.MonthlyFee;

                existingTariff.Services.Clear();

                var serviceIds = tariff.Services.Select(s => s.Id).ToList();
                var services = db.Services
                    .Where(s => serviceIds.Contains(s.Id))
                    .ToList();

                foreach (var service in services)
                    existingTariff.Services.Add(service);

                db.SaveChanges();
            }
        }

        public void RemoveTariff(Tariff tariff)
        {
            ArgumentNullException.ThrowIfNull(tariff);

            using (var db = new TelecomDbContext())
            {
                var existingTariff = db.Tariffs
                    .FirstOrDefault(t => t.Id == tariff.Id) ?? throw new Exception("Тариф не найден");

                db.Tariffs.Remove(existingTariff);
                db.SaveChanges();
            }
        }

        public List<Tariff> GetAll()
        {
            using (var db = new TelecomDbContext())
            {
                return db.Tariffs
                    .Include(t => t.Services)
                    .ToList();
            }
        }
    }
}
