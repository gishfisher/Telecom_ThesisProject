using System;
using System.Collections.Generic;

namespace Telecom_ThesisProject.MVVM.Model;

public partial class Request
{
    public int Id { get; set; }

    public string? Description { get; set; }

    public int TypeId { get; set; }

    public int? StatusId { get; set; }

    public int ClientId { get; set; }

    public int? ConnectionId { get; set; }

    public int? DeviceId { get; set; }

    public int? EmployeeId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ClosedAt { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual Connection? Connection { get; set; }

    public virtual NetworkDevice? Device { get; set; }

    public virtual Employee? Employee { get; set; }

    public virtual ICollection<RequestComment> RequestComments { get; set; } = new List<RequestComment>();

    public virtual RequestStatus? Status { get; set; }

    public virtual RequestsType Type { get; set; } = null!;
}
