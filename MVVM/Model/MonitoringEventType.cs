using System;
using System.Collections.Generic;

namespace Telecom_ThesisProject.MVVM.Model;

public partial class MonitoringEventType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<MonitoringEvent> MonitoringEvents { get; set; } = new List<MonitoringEvent>();
}
