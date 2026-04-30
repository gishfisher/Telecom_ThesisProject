using System;
using System.Collections.Generic;

namespace Telecom_ThesisProject.MVVM.Model;

public partial class RequestComment
{
    public int Id { get; set; }

    public int RequestId { get; set; }

    public string Comment { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public int CreatedBy { get; set; }

    public virtual Employee CreatedByNavigation { get; set; } = null!;

    public virtual Request Request { get; set; } = null!;
}
