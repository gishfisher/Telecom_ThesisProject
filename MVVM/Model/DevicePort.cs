using System;
using System.Collections.Generic;

namespace Telecom_ThesisProject.MVVM.Model;

public partial class DevicePort
{
    public int Id { get; set; }

    public int DeviceId { get; set; }

    public int PortNumber { get; set; }

    public bool? IsUplink { get; set; }

    public virtual ICollection<Connection> Connections { get; set; } = new List<Connection>();

    public virtual NetworkDevice Device { get; set; } = null!;
}
