using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Telecom_ThesisProject.Data;
using Telecom_ThesisProject.MVVM.Model;

namespace Telecom_ThesisProject.Services;

class NetworkDeviceService
{
    // === Network Devices ===

    public void AddDevice(NetworkDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);
        DetachNavigations(device);

        using (var db = new TelecomDbContext())
        {
            db.NetworkDevices.Add(device);
            db.SaveChanges();
        }
    }

    public void EditDevice(NetworkDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);
        DetachNavigations(device);

        using (var db = new TelecomDbContext())
        {
            var existing = db.NetworkDevices
                .FirstOrDefault(d => d.Id == device.Id) ?? throw new Exception("Устройство не найдено");

            existing.Name = device.Name;
            existing.DeviceTypeId = device.DeviceTypeId;
            existing.IpAddress = device.IpAddress;
            existing.MountingPointId = device.MountingPointId;
            existing.SnmpProfileId = device.SnmpProfileId;
            existing.ParentDeviceId = device.ParentDeviceId;
            existing.IsMonitored = device.IsMonitored;
            existing.InstallationDate = device.InstallationDate;

            db.SaveChanges();
        }
    }
    public void RemoveDevice(NetworkDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);

        using (var db = new TelecomDbContext())
        {
            var existing = db.NetworkDevices
                .FirstOrDefault(d => d.Id == device.Id) ?? throw new Exception("Устройство не найдено");

            var children = db.NetworkDevices.Where(d => d.ParentDeviceId == existing.Id).ToList();
            foreach (var c in children)
                c.ParentDeviceId = null;

            db.SaveChanges();

            db.NetworkDevices.Remove(existing);
            db.SaveChanges();
        }
    }

    // === Getters ===
    public NetworkDevice? GetDeviceById(int id)
    {
        using (var db = new TelecomDbContext())
        {
            return db.NetworkDevices
                .AsNoTracking()
                .Include(d => d.MountingPoint)
                    .ThenInclude(m => m.Address)
                        .ThenInclude(a => a.Street)
                            .ThenInclude(s => s.City)
                .Include(d => d.DeviceType)
                .Include(d => d.ParentDevice)
                .FirstOrDefault(d => d.Id == id);
        }
    }

    public List<NetworkDevice> GetAllDevices()
    {
        using (var db = new TelecomDbContext())
        {
            return db.NetworkDevices
                .Include(d => d.DeviceType)
                .Include(d => d.MountingPoint)
                .Include(d => d.ParentDevice)
                .OrderBy(d => d.Name)
                .ToList();
        }
    }

    public List<DeviceType> GetDeviceTypes()
    {
        using (var db = new TelecomDbContext())
        {
            return db.DeviceTypes.OrderBy(t => t.TypeName).ToList();
        }
    }

    public List<MountingPoint> GetMountingPointsWithAddress()
    {
        using (var db = new TelecomDbContext())
        {
            return db.MountingPoints
                .Include(m => m.Address)
                .ThenInclude(a => a.Street)
                .ThenInclude(s => s.City)
                .Include(m => m.PointType)
                .OrderBy(m => m.Id)
                .ToList();
        }
    }

    public List<NetworkDevice> GetParentCandidates(int? excludeDeviceId)
    {
        using (var db = new TelecomDbContext())
        {
            var q = db.NetworkDevices.AsQueryable();
            if (excludeDeviceId.HasValue && excludeDeviceId.Value > 0)
                q = q.Where(d => d.Id != excludeDeviceId.Value);
            return q.OrderBy(d => d.Name).ToList();
        }
    }

    #region Others Getters

    //public List<NetworkDevice> GetDevicesByMountingPoint(int mountingPointId)
    //{
    //    using (var db = new TelecomDbContext())
    //    {
    //        return db.NetworkDevices
    //            .Where(d => d.MountingPointId == mountingPointId)
    //            .Include(d => d.DeviceType)
    //            .Include(d => d.ParentDevice)
    //            .OrderBy(d => d.Name)
    //            .ToList();
    //    }
    //}

    //public List<NetworkDevice> GetDevicesByParent(int parentDeviceId)
    //{
    //    using (var db = new TelecomDbContext())
    //    {
    //        return db.NetworkDevices
    //            .Where(d => d.ParentDeviceId == parentDeviceId)
    //            .Include(d => d.DeviceType)
    //            .Include(d => d.MountingPoint)
    //            .OrderBy(d => d.Name)
    //            .ToList();
    //    }
    //}

    //public List<NetworkDevice> GetDevicesForParent(int? excludeDeviceId)
    //{
    //    using (var db = new TelecomDbContext())
    //    {
    //        var q = db.NetworkDevices.AsQueryable();
    //        if (excludeDeviceId.HasValue && excludeDeviceId.Value > 0)
    //            q = q.Where(d => d.Id != excludeDeviceId.Value);
    //        return q.OrderBy(d => d.Name).ToList();
    //    }
    //}

    #endregion

    // === SNMP ===

    public void SetSnmpProfile(int deviceId, int? snmpProfileId)
    {
        using (var db = new TelecomDbContext())
        {
            var device = db.NetworkDevices.FirstOrDefault(d => d.Id == deviceId) ?? throw new Exception("Устройство не найдено");
            device.SnmpProfileId = snmpProfileId;
            db.SaveChanges();
        }
    }

    public void AddSnmpProfile(SnmpProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        using (var db = new TelecomDbContext())
        {
            db.SnmpProfiles.Add(profile);
            db.SaveChanges();
        }
    }

    public void EditSnmpProfile(SnmpProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        using (var db = new TelecomDbContext())
        {
            var existing = db.SnmpProfiles.FirstOrDefault(p => p.Id == profile.Id) ?? throw new Exception("Профиль не найден");
            existing.Name = profile.Name;
            existing.Community = profile.Community;
            existing.Version = profile.Version;
            db.SaveChanges();
        }
    }

    public void DeleteSnmpProfile(SnmpProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        using (var db = new TelecomDbContext())
        {
            var existing = db.SnmpProfiles.FirstOrDefault(p => p.Id == profile.Id) ?? throw new Exception("Профиль не найден");
            var devices = db.NetworkDevices.Where(d => d.SnmpProfileId == profile.Id).ToList();
            foreach (var d in devices)
                d.SnmpProfileId = null;
            db.SaveChanges();
            db.SnmpProfiles.Remove(existing);
            db.SaveChanges();
        }
    }

    // === Getters ===

    public List<SnmpProfile> GetSnmpProfiles()
    {
        using (var db = new TelecomDbContext())
        {
            return db.SnmpProfiles.OrderBy(p => p.Name).ToList();
        }
    }

    // === Helpers ===

    // Detach навигационных свойств, чтобы избежать проблем с отслеживанием сущностей при добавлении/редактировании
    private static void DetachNavigations(NetworkDevice device)
    {
        device.DeviceType = null!;
        device.MountingPoint = null!;
        device.ParentDevice = null;
        device.DevicePorts = new List<DevicePort>();
        device.InverseParentDevice = new List<NetworkDevice>();
        device.Requests = new List<Request>();
    }
}
