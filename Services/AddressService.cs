using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Telecom_ThesisProject.Data;
using Telecom_ThesisProject.MVVM.Model;

namespace Telecom_ThesisProject.Services
{
    class AddressService
    {
        // === Cities ===

        // Добавить город
        public void AddCity(City city)
        {
            ArgumentNullException.ThrowIfNull(city);
            using (var db = new TelecomDbContext())
            {
                db.Cities.Add(city);
                db.SaveChanges();
            }
        }

        // Редактировать город
        public void EditCity(City city)
        {
            ArgumentNullException.ThrowIfNull(city);
            using (var db = new TelecomDbContext())
            {
                var existing = db.Cities.FirstOrDefault(c => c.Id == city.Id)
                    ?? throw new Exception("Город не найден");
                existing.Name = city.Name;
                db.SaveChanges();
            }
        }

        // Удалить город
        public void RemoveCity(City city)
        {
            ArgumentNullException.ThrowIfNull(city);
            using (var db = new TelecomDbContext())
            {
                var existing = db.Cities
                    .Include(c => c.Streets).ThenInclude(s => s.Addresses)
                    .FirstOrDefault(c => c.Id == city.Id)
                    ?? throw new Exception("Город не найден");

                db.Cities.Remove(existing);
                db.SaveChanges();
            }
        }

        // === Getters ===

        // Получить все города с их улицами, адресами и квартирами
        public List<City> GetAllCities()
        {
            using (var db = new TelecomDbContext())
            {
                return db.Cities
                    .Include(c => c.Streets)
                    .ThenInclude(s => s.Addresses)
                    .ThenInclude(a => a.Apartments)
                    .OrderBy(c => c.Name)
                    .ToList();
            }
        }

        // Получить все города для отображения в комбобоксе
        public List<City> GetCitiesForCombo()
        {
            using (var db = new TelecomDbContext())
            {
                return db.Cities.OrderBy(c => c.Name).ToList();
            }
        }

        // Получить города по id
        public List<City> GetCitiesById(int id)
        {
            using (var db = new TelecomDbContext())
            {
                return db.Cities.Where(c => c.Id == id).ToList();
            }
        }

        // === Streets ===

        // Добавить улицу
        public void AddStreet(Street street)
        {
            ArgumentNullException.ThrowIfNull(street);
            using (var db = new TelecomDbContext())
            {
                db.Streets.Add(street);
                db.SaveChanges();
            }
        }

        // Редактировать улицу
        public void EditStreet(Street street)
        {
            ArgumentNullException.ThrowIfNull(street);
            using (var db = new TelecomDbContext())
            {
                var existing = db.Streets.FirstOrDefault(s => s.Id == street.Id)
                    ?? throw new Exception("Улица не найдена");
                existing.Name = street.Name;
                existing.CityId = street.CityId;
                db.SaveChanges();
            }
        }

        // Удалить улицу
        public void RemoveStreet(Street street)
        {
            ArgumentNullException.ThrowIfNull(street);
            using (var db = new TelecomDbContext())
            {
                var existing = db.Streets
                    .Include(s => s.Addresses)
                    .FirstOrDefault(s => s.Id == street.Id)
                    ?? throw new Exception("Улица не найдена");

                db.Streets.Remove(existing);
                db.SaveChanges();
            }
        }

        // === Getters ===

        // Получить все улицы по id города
        public List<Street> GetStreetsByCityId(int cityId)
        {
            using (var db = new TelecomDbContext())
            {
                return db.Streets.Where(s => s.CityId == cityId)
                         .Include(s => s.Addresses)
                         .OrderBy(s => s.Name)
                         .ToList();
            }
        }

        // === Addreses ===

        // Добавить адрес
        public void AddAddress(Address address)
        {
            ArgumentNullException.ThrowIfNull(address);
            using (var db = new TelecomDbContext())
            {
                db.Addresses.Add(address);
                db.SaveChanges();
            }
        }

        // Редактировать адрес
        public void EditAddress(Address address)
        {
            ArgumentNullException.ThrowIfNull(address);
            using (var db = new TelecomDbContext())
            {
                var existing = db.Addresses.FirstOrDefault(a => a.Id == address.Id)
                    ?? throw new Exception("Адрес не найден");
                existing.HouseNumber = address.HouseNumber;
                existing.StreetId = address.StreetId;
                db.SaveChanges();
            }
        }

        // Удалить адрес
        public void RemoveAddress(Address address)
        {
            ArgumentNullException.ThrowIfNull(address);
            using (var db = new TelecomDbContext())
            {
                var existing = db.Addresses
                    .FirstOrDefault(a => a.Id == address.Id)
                    ?? throw new Exception("Адрес не найден");

                // хз

                bool hasClients = db.Clients.Any(c => c.Connections.FirstOrDefault().ApartmentId == existing.Apartments.FirstOrDefault().Id);
                if (hasClients)
                    throw new InvalidOperationException("Есть клиенты, привязанные к адресу.");

                db.Addresses.Remove(existing);
                db.SaveChanges();
            }
        }

        // === Getters ===

        // Получить все адреса по id улицы
        public List<Address> GetAddressesByStreetId(int streetId)
        {
            using (var db = new TelecomDbContext())
            {
                return db.Addresses.Where(s => s.StreetId == streetId)
                                   .OrderBy(s => s.HouseNumber)
                                   .ToList();
            }
        }

        // === Apartments ===

        // Добавить квартиру
        public void AddApartment(Apartment apartment)
        {
            ArgumentNullException.ThrowIfNull(apartment);
            using (var db = new TelecomDbContext())
            {
                db.Apartments.Add(apartment);
                db.SaveChanges();
            }
        }

        // Редактировать квартиру
        public void EditApartment(Apartment apartment)
        {
            ArgumentNullException.ThrowIfNull(apartment);
            using (var db = new TelecomDbContext())
            {
                var existing = db.Apartments.FirstOrDefault(a => a.Id == apartment.Id)
                    ?? throw new Exception("Квартира не найдена");
                existing.Number = apartment.Number;
                existing.AddressId = apartment.AddressId;
                db.SaveChanges();
            }
        }

        // Удалить квартиру
        public void RemoveApartment(Apartment apartment)
        {
            ArgumentNullException.ThrowIfNull(apartment);
            using (var db = new TelecomDbContext())
            {
                var existing = db.Apartments.FirstOrDefault(a => a.Id == apartment.Id)
                    ?? throw new Exception("Квартира не найдена");
                bool hasClients = db.Clients.FirstOrDefault()?.Connections.Any(c => c.ApartmentId == existing.Id) ?? false;
                if (hasClients)
                    throw new InvalidOperationException("Есть клиенты, привязанные к квартире.");
                db.Apartments.Remove(existing);
                db.SaveChanges();
            }
        }

        // === Getters ===

        // Получить квартиру по id адреса и номеру
        public Apartment GetApartmentIdByAddressIdAndNumber(int addressId, string number)
        {
            using (var db = new TelecomDbContext())
            {
                return db.Apartments.FirstOrDefault(a => a.AddressId == addressId && a.Number == number) 
                    ?? throw new Exception("Квартира не найдена");
            }
        }

        // Получить все квартиры по id адреса
        public List<Apartment> GetApartmentsByAddressId(int addressId)
        {
            using (var db = new TelecomDbContext())
            {
                return db.Apartments.Where(a => a.AddressId == addressId)
                                    .OrderBy(a => a.Number)
                                    .ToList();
            }
        }

        // Получить все квартиры по id улицы
        public bool ApartamentIsExist(int addressId, string number)
        {
            using (var db = new TelecomDbContext())
            {
                return db.Apartments.Any(a => a.AddressId == addressId && a.Number == number);
            }
        }

        // Получить все адреса с их улицами, городами и квартирами
        public List<Address> GetAllWithLocation()
        {
            using (var db = new TelecomDbContext())
            {
                return db.Addresses
                    .AsNoTracking()
                    .Include(a => a.Street)
                    .ThenInclude(s => s.City)
                    .Include(a => a.Apartments)
                    .OrderBy(a => a.Id)
                    .ToList();
            }
        }

        // Получить все адреса с их улицами, городами, квартирами и точками монтажа
        public List<Address> GetAddressesWithInclude()
        {
            using (var db = new TelecomDbContext())
            {
                return db.Addresses
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Include(a => a.Apartments)
                    .Include(a => a.Street)
                        .ThenInclude(s => s.City)
                    .Include(a => a.MountingPoints)
                        .ThenInclude(mp => mp.PointType)
                    .Include(a => a.MountingPoints)
                        .ThenInclude(mp => mp.SpotType)
                    .Include(a => a.MountingPoints)
                        .ThenInclude(mp => mp.NetworkDevices)
                            .ThenInclude(tp => tp.DeviceType)
                    .ToList();
            }
        }

        // === Mounting Point ===

        // Добавить точку монтажа
        public void AddMountingPoint(MountingPoint mp)
        {
            ArgumentNullException.ThrowIfNull(mp);

            using (var db = new TelecomDbContext())
            {
                db.MountingPoints.Add(mp);
                db.SaveChanges();
            }
        }

        // Редактировать точку монтажа
        public void EditMountingPoint(MountingPoint mp)
        {
            ArgumentNullException.ThrowIfNull(mp);
            using (var db = new TelecomDbContext())
            {
                var existing = db.MountingPoints.FirstOrDefault(m => m.Id == mp.Id)
                    ?? throw new Exception("Точка монтажа не найдена");
                existing.PointTypeId = mp.PointTypeId;
                existing.LocationDescription = mp.LocationDescription;
                existing.AddressId = mp.AddressId;
                db.SaveChanges();
            }
        }

        // Удалить точку монтажа
        public void RemoveMountingPoint(MountingPoint mp)
        {
            ArgumentNullException.ThrowIfNull(mp);
            using (var db = new TelecomDbContext())
            {
                var existing = db.MountingPoints
                    .Include(m => m.NetworkDevices)
                    .FirstOrDefault(m => m.Id == mp.Id)
                    ?? throw new Exception("Точка монтажа не найдена");

                foreach (var device in existing.NetworkDevices)
                    device.MountingPointId = null;
                db.SaveChanges();

                foreach (var device in existing.NetworkDevices)
                {
                    var children = db.NetworkDevices.Where(d => d.ParentDeviceId == device.Id).ToList();
                    foreach (var child in children)
                        child.ParentDeviceId = null;
                }
                db.SaveChanges();

                db.MountingPoints.Remove(existing);
                db.SaveChanges();
            }
        }

        // === Getters ===

        // Получить все точки монтажа по id адреса
        public List<MountingPointType> GetMountingPointTypes()
        {
            using (var db = new TelecomDbContext())
            {
                return db.MountingPointTypes.OrderBy(t => t.Name).ToList();
            }
        }

        // Получить точку монтажа по id
        public MountingPoint? GetMountingPointsById(int id)
        {
            using (var db = new TelecomDbContext())
            {
                return db.MountingPoints.FirstOrDefault(mp => mp.Id == id);
            }
        }
    }
}