using System;
using System.Collections.Generic;

namespace Telecom_ThesisProject.MVVM.Model;

public partial class NetworkDevice
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int DeviceTypeId { get; set; }

    public string? Vendor { get; set; }

    public string? Model { get; set; }

    public string? SerialNumber { get; set; }

    public string IpAddress { get; set; } = null!;

    public string? SnmpCommunity { get; set; }

    public int MountingPointId { get; set; }

    public int? ParentDeviceId { get; set; }

    public bool? IsMonitored { get; set; }

    public DateOnly? InstallationDate { get; set; }

    public virtual ICollection<DevicePort> DevicePorts { get; set; } = new List<DevicePort>();

    public virtual DeviceType DeviceType { get; set; } = null!;

    public virtual ICollection<NetworkDevice> InverseParentDevice { get; set; } = new List<NetworkDevice>();

    public virtual MountingPoint MountingPoint { get; set; } = null!;

    public virtual NetworkDevice? ParentDevice { get; set; }

    public virtual ICollection<Request> Requests { get; set; } = new List<Request>();
}
