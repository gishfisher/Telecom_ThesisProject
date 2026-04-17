using System;
using System.Collections.Generic;

namespace Telecom_ThesisProject.MVVM.Model;

public partial class SnmpProfile
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Community { get; set; } = null!;

    public int Port { get; set; }

    public string Version { get; set; } = null!;

    public virtual ICollection<NetworkDevice> NetworkDevices { get; set; } = new List<NetworkDevice>();

    public string SnmpProfileString =>
        string.Join(", ", new[] { Name, $"Версия: {Version}" }.Where(s => !string.IsNullOrWhiteSpace(s))).Trim();
}
