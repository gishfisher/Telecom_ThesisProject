using System;
using System.Collections.Generic;

namespace Telecom_ThesisProject.MVVM.Model;

public partial class MountingPoint
{
    public int Id { get; set; }

    public int AddressId { get; set; }

    public int PointTypeId { get; set; }

    public string? LocationDescription { get; set; }

    public bool? HasPowerBackup { get; set; }

    public virtual Address Address { get; set; } = null!;

    public virtual ICollection<NetworkDevice> NetworkDevices { get; set; } = new List<NetworkDevice>();

    public virtual MountingPointType PointType { get; set; } = null!;

    /// <summary>Краткая подпись для списков (нужны загруженные PointType, Address).</summary>
    public string DisplayLabel =>
        $"{PointType?.Name ?? "Точка"} — {LocationDescription ?? "—"} (ID {Id})";
}
