using System;
using System.Collections.Generic;
using System.Linq;

namespace Telecom_ThesisProject.MVVM.Model;

public partial class Client
{
    public int Id { get; set; }

    public string LastName { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? MiddleName { get; set; }

    public string ContractNumber { get; set; } = null!;

    public int AddressId { get; set; }

    public decimal? Balance { get; set; }

    public virtual Address Address { get; set; } = null!;

    public virtual ICollection<Connection> Connections { get; set; } = new List<Connection>();

    public virtual ICollection<Request> Requests { get; set; } = new List<Request>();

    /// <summary>ФИО для отображения в списках и привязках.</summary>
    public string FullName =>
        string.Join(" ", new[] { LastName, FirstName, MiddleName }.Where(s => !string.IsNullOrWhiteSpace(s))).Trim();
}
