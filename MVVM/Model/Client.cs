using System;
using System.Collections.Generic;

namespace Telecom_ThesisProject.MVVM.Model;

public partial class Client
{
    public int Id { get; set; }

    public string LastName { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? MiddleName { get; set; }

    public string? PhoneNumber { get; set; }

    public virtual ICollection<Connection> Connections { get; set; } = new List<Connection>();

    public virtual ICollection<Request> Requests { get; set; } = new List<Request>();

    public string GetFullName =>
        string.Join(" ", new[] { LastName, FirstName, MiddleName }.Where(s => !string.IsNullOrWhiteSpace(s))).Trim();

    public string GetFullNameIn =>
        $"{LastName} {FirstName[0]}.{MiddleName?[0]}.";

    public string FormattedPhone => string.IsNullOrEmpty(PhoneNumber) ? string.Empty
    : $"+{PhoneNumber[0]} ({PhoneNumber[1..4]}) {PhoneNumber[4..7]}-{PhoneNumber[7..9]}-{PhoneNumber[9..11]}";
}
