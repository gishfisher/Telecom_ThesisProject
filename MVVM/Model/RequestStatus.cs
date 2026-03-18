using System;
using System.Collections.Generic;

namespace Telecom_ThesisProject.MVVM.Model;

public partial class RequestStatus
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Request> Requests { get; set; } = new List<Request>();
}
