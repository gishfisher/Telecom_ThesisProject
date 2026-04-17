using System;
using System.Collections.Generic;
using Telecom_ThesisProject.Core;

namespace Telecom_ThesisProject.MVVM.Model;

public partial class NetworkDevice : ObservableObject
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int DeviceTypeId { get; set; }

    public string IpAddress { get; set; } = null!;

    public int? SnmpProfileId { get; set; }

    public int? MountingPointId { get; set; }

    public int? ParentDeviceId { get; set; }

    public bool? IsMonitored { get; set; }

    public DateOnly? InstallationDate { get; set; }

    public virtual ICollection<DevicePort> DevicePorts { get; set; } = new List<DevicePort>();

    public virtual DeviceType DeviceType { get; set; } = null!;

    public virtual ICollection<NetworkDevice> InverseParentDevice { get; set; } = new List<NetworkDevice>();

    public virtual ICollection<MonitoringEvent> MonitoringEvents { get; set; } = new List<MonitoringEvent>();

    public virtual MountingPoint? MountingPoint { get; set; }

    public virtual NetworkDevice? ParentDevice { get; set; }

    public virtual ICollection<Request> Requests { get; set; } = new List<Request>();

    public virtual SnmpProfile? SnmpProfile { get; set; }
}
