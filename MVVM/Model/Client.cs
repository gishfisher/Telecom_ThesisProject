using System;
using System.Collections.Generic;

namespace Telecom_ThesisProject.MVVM.Model;

public partial class Client
{
    public int Id { get; set; }

    public string FullName { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string ContractNumber { get; set; } = null!;

    public virtual ICollection<Connection> Connections { get; set; } = new List<Connection>();

    public virtual ICollection<Request> Requests { get; set; } = new List<Request>();
}
