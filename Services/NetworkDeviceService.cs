using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Telecom_ThesisProject.Data;
using Telecom_ThesisProject.MVVM.Model;

namespace Telecom_ThesisProject.Services;

class NetworkDeviceService
{
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
            existing.SnmpCommunity = device.SnmpCommunity;
            existing.MountingPointId = device.MountingPointId;
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

    public List<NetworkDevice> GetAll()
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
