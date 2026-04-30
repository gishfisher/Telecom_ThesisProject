using System;
using System.Collections.Generic;

namespace Telecom_ThesisProject.MVVM.Model;

public partial class Employee
{
    public int Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? MiddleName { get; set; }

    public int? UserId { get; set; }

    public virtual ICollection<RequestComment> RequestComments { get; set; } = new List<RequestComment>();

    public virtual ICollection<Request> Requests { get; set; } = new List<Request>();

    public virtual User? User { get; set; }

    public string GetFullName =>
        string.Join(" ", new[] { LastName, FirstName, MiddleName }.Where(s => !string.IsNullOrWhiteSpace(s))).Trim();
}
