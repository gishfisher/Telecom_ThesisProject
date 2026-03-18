using System;
using System.Collections.Generic;

namespace Telecom_ThesisProject.MVVM.Model;

public partial class Connection
{
    public int Id { get; set; }

    public int ClientId { get; set; }

    public int PortId { get; set; }

    public int TariffId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsActive { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual DevicePort Port { get; set; } = null!;

    public virtual Tariff Tariff { get; set; } = null!;
}
