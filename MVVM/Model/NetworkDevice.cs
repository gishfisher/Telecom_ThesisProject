using System;
using System.Collections.Generic;

namespace Telecom_ThesisProject.MVVM.Model;

public partial class NetworkDevice
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string IpAddress { get; set; } = null!;

    public string? DeviceType { get; set; }

    public string? Location { get; set; }

    public string? LastUpTime { get; set; }

    public int? ParentDeviceId { get; set; }

    public virtual ICollection<DevicePort> DevicePorts { get; set; } = new List<DevicePort>();

    public virtual ICollection<NetworkDevice> InverseParentDevice { get; set; } = new List<NetworkDevice>();

    public virtual NetworkDevice? ParentDevice { get; set; }

    public virtual ICollection<Request> Requests { get; set; } = new List<Request>();
}
