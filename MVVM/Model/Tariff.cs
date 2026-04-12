using System;
using System.Collections.Generic;

namespace Telecom_ThesisProject.MVVM.Model;

public partial class Tariff
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal? MonthlyFee { get; set; }

    public virtual ICollection<Connection> Connections { get; set; } = new List<Connection>();

    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
}
