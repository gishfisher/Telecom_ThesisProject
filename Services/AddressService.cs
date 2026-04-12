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
        public List<Address> GetAllWithLocation()
        {
            using (var db = new TelecomDbContext())
            {
                return db.Addresses
                    .Include(a => a.Street)
                    .ThenInclude(s => s.City)
                    .OrderBy(a => a.Id)
                    .ToList();
            }
        }

        // Cities
        public List<City> GetAllCities()
        {
            using (var db = new TelecomDbContext())
            {
                return db.Cities
                    .Include(c => c.Streets)
                    .ThenInclude(s => s.Addresses)
                    .OrderBy(c => c.Name)
                    .ToList();
            }
        }

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

        // Street
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

        //Address
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
                existing.Apartment = address.Apartment;
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
                    .Include(a => a.Clients)
                    .FirstOrDefault(a => a.Id == address.Id)
                    ?? throw new Exception("Адрес не найден");

                db.Addresses.Remove(existing);
                db.SaveChanges();
            }
        }

        public List<City> GetCitiesForCombo()
        {
            using (var db = new TelecomDbContext())
            {
                return db.Cities.OrderBy(c => c.Name).ToList();
            }
        }

        public List<Street> GetStreetsByCityId(int cityId)
        {
            using (var db = new TelecomDbContext())
            {
                return db.Streets.Where(s => s.CityId == cityId).OrderBy(s => s.Name).ToList();
            }
        }

        //Mounting Point
        public List<MountingPointType> GetMountingPointTypes()
        {
            using (var db = new TelecomDbContext())
            {
                return db.MountingPointTypes.OrderBy(t => t.Name).ToList();
            }
        }

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

        //NetworkDevice
        public List<DeviceType> GetDeviceTypes()
        {
            using (var db = new TelecomDbContext())
            {
                return db.DeviceTypes.OrderBy(t => t.TypeName).ToList();
            }
        }

        public List<NetworkDevice> GetDevicesForParent(int? excludeDeviceId)
        {
            using (var db = new TelecomDbContext())
            {
                var q = db.NetworkDevices.AsQueryable();
                if (excludeDeviceId.HasValue && excludeDeviceId.Value > 0)
                    q = q.Where(d => d.Id != excludeDeviceId.Value);
                return q.OrderBy(d => d.Name).ToList();
            }
        }

        public void AddNetworkDevice(NetworkDevice device)
        {
            ArgumentNullException.ThrowIfNull(device);
            using (var db = new TelecomDbContext())
            {
                db.NetworkDevices.Add(device);
                db.SaveChanges();
            }
        }

        public void EditNetworkDevice(NetworkDevice device)
        {
            ArgumentNullException.ThrowIfNull(device);
            using (var db = new TelecomDbContext())
            {
                var existing = db.NetworkDevices.FirstOrDefault(d => d.Id == device.Id)
                    ?? throw new Exception("Устройство не найдено");
                existing.Name = device.Name;
                existing.DeviceTypeId = device.DeviceTypeId;
                existing.IpAddress = device.IpAddress;
                existing.SnmpCommunity = device.SnmpCommunity;
                existing.MountingPointId = device.MountingPointId;
                existing.ParentDeviceId = device.ParentDeviceId;
                existing.IsMonitored = device.IsMonitored;
                existing.InstallationDate = device.InstallationDate;
                db.SaveChanges();
            }
        }

        public void RemoveNetworkDevice(NetworkDevice device)
        {
            ArgumentNullException.ThrowIfNull(device);
            using (var db = new TelecomDbContext())
            {
                var existing = db.NetworkDevices
                    .Include(d => d.DevicePorts)
                        .ThenInclude(p => p.Connection)
                    .FirstOrDefault(d => d.Id == device.Id)
                    ?? throw new Exception("Устройство не найдено");

                var children = db.NetworkDevices.Where(d => d.ParentDeviceId == existing.Id).ToList();
                foreach (var child in children)
                    child.ParentDeviceId = null;
                db.SaveChanges();

                db.NetworkDevices.Remove(existing);
                db.SaveChanges();
            }
        }
    }
}
