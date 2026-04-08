using System;
using System.Collections.Generic;

namespace Telecom_ThesisProject.MVVM.Model;

public partial class TrafficBillingLog
{
    public long Id { get; set; }

    public int ConnectionId { get; set; }

    public DateTime? Timestamp { get; set; }

    public long BytesInDelta { get; set; }

    public decimal CostCharged { get; set; }

    public virtual Connection Connection { get; set; } = null!;
}
