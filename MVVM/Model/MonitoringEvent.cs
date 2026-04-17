using System;
using System.Collections.Generic;

namespace Telecom_ThesisProject.MVVM.Model;

public partial class MonitoringEvent
{
    public long Id { get; set; }

    public int DeviceId { get; set; }

    public DateTime CheckedAt { get; set; }

    public bool IsOnline { get; set; }

    public int? ResponseTimeMs { get; set; }

    public int EventTypeId { get; set; }

    public virtual NetworkDevice Device { get; set; } = null!;

    public virtual MonitoringEventType EventType { get; set; } = null!;
}
