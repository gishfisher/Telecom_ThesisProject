using System;
using System.Collections.Generic;

namespace Telecom_ThesisProject.MVVM.Model;

public partial class Connection
{
    public int Id { get; set; }

    public string StaticIp { get; set; } = null!;

    public int ClientId { get; set; }

    public int ApartmentId { get; set; }

    public int PortId { get; set; }

    public int TariffId { get; set; }

    public bool IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Apartment Apartment { get; set; } = null!;

    public virtual Client Client { get; set; } = null!;

    public virtual DevicePort Port { get; set; } = null!;

    public virtual Tariff Tariff { get; set; } = null!;

    public string GetConnectionString => $"Подключение №{Id}, {Client.GetFullNameIn}";
}
