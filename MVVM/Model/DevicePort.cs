using System;
using System.Collections.Generic;

namespace Telecom_ThesisProject.MVVM.Model;

public partial class DevicePort
{
    public int Id { get; set; }

    public int DeviceId { get; set; }

    public string? PortName { get; set; }

    public int? VlanId { get; set; }

    public bool IsUplink { get; set; }

    public virtual Connection? Connection { get; set; }

    public virtual NetworkDevice Device { get; set; } = null!;

    public virtual Vlan? Vlan { get; set; }
}
