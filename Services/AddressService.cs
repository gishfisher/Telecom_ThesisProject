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

        public void AddCity(City city)
        {
            ArgumentNullException.ThrowIfNull(city);
            using (var db = new TelecomDbContext())
            {
                db.Cities.Add(city);
                db.SaveChanges();
            }
        }

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

        public List<City> GetCitiesForCombo()
        {
            using (var db = new TelecomDbContext())
            {
                return db.Cities.OrderBy(c => c.Name).ToList();
            }
        }

        public List<City> GetCitiesById(int id)
        {
            using (var db = new TelecomDbContext())
            {
                return db.Cities.Where(c => c.Id == id).ToList();
            }
        }

        // === Streets ===

        public void AddStreet(Street street)
        {
            ArgumentNullException.ThrowIfNull(street);
            using (var db = new TelecomDbContext())
            {
                db.Streets.Add(street);
                db.SaveChanges();
            }
        }

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

        public void AddAddress(Address address)
        {
            ArgumentNullException.ThrowIfNull(address);
            using (var db = new TelecomDbContext())
            {
                db.Addresses.Add(address);
                db.SaveChanges();
            }
        }

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

        public void RemoveAddress(Address address)
        {
            ArgumentNullException.ThrowIfNull(address);
            using (var db = new TelecomDbContext())
            {
                var existing = db.Addresses
                    .FirstOrDefault(a => a.Id == address.Id)
                    ?? throw new Exception("Адрес не найден");

                // хз

                //bool hasClients = db.Clients.Any(c => c.Connections.FirstOrDefault().ApartmentId == existing.Apartments.FirstOrDefault().Id);
                //if (hasClients)
                //    throw new InvalidOperationException("Есть клиенты, привязанные к адресу.");

                db.Addresses.Remove(existing);
                db.SaveChanges();
            }
        }

        // === Apartments ===

        public void AddApartment(Apartment apartment)
        {
            ArgumentNullException.ThrowIfNull(apartment);
            using (var db = new TelecomDbContext())
            {
                db.Apartments.Add(apartment);
                db.SaveChanges();
            }
        }

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

        public List<Address> GetAddressesByStreetId(int streetId)
        {
            using (var db = new TelecomDbContext())
            {
                return db.Addresses.Where(s => s.StreetId == streetId)
                                   .OrderBy(s => s.HouseNumber)
                                   .ToList();
            }
        }

        public Apartment GetApartmentIdByAddressIdAndNumber(int addressId, string number)
        {
            using (var db = new TelecomDbContext())
            {
                return db.Apartments.FirstOrDefault(a => a.AddressId == addressId && a.Number == number) 
                    ?? throw new Exception("Квартира не найдена");
            }
        }

        public List<Apartment> GetApartmentsByAddressId(int addressId)
        {
            using (var db = new TelecomDbContext())
            {
                return db.Apartments.Where(a => a.AddressId == addressId)
                                    .OrderBy(a => a.Number)
                                    .ToList();
            }
        }

        public bool ApartamentIsExist(int addressId, string number)
        {
            using (var db = new TelecomDbContext())
            {
                return db.Apartments.Any(a => a.AddressId == addressId && a.Number == number);
            }
        }

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

        public void AddMountingPoint(MountingPoint mp)
        {
            ArgumentNullException.ThrowIfNull(mp);

            using (var db = new TelecomDbContext())
            {
                db.MountingPoints.Add(mp);
                db.SaveChanges();
            }
        }

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

        public List<MountingPointType> GetMountingPointTypes()
        {
            using (var db = new TelecomDbContext())
            {
                return db.MountingPointTypes.OrderBy(t => t.Name).ToList();
            }
        }

        public MountingPoint? GetMountingPointsById(int id)
        {
            using (var db = new TelecomDbContext())
            {
                return db.MountingPoints.FirstOrDefault(mp => mp.Id == id);
            }
        }
    }
}