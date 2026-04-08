using System;
using System.Collections.Generic;

namespace Telecom_ThesisProject.MVVM.Model;

public partial class DeviceType
{
    public int Id { get; set; }

    public string TypeName { get; set; } = null!;

    public virtual ICollection<NetworkDevice> NetworkDevices { get; set; } = new List<NetworkDevice>();
}
