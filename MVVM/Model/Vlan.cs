using System;
using System.Collections.Generic;

namespace Telecom_ThesisProject.MVVM.Model;

public partial class Vlan
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int VlanTag { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<DevicePort> DevicePorts { get; set; } = new List<DevicePort>();
}
