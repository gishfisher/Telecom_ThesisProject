using System;
using System.Collections.Generic;

namespace Telecom_ThesisProject.MVVM.Model;

public partial class Apartment
{
    public int Id { get; set; }

    public int AddressId { get; set; }

    public string Number { get; set; } = null!;

    public virtual Address? Address { get; set; }

    public virtual ICollection<Connection> Connections { get; set; } = new List<Connection>();
}
