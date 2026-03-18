using System;
using System.Collections.Generic;

namespace Telecom_ThesisProject.MVVM.Model;

public partial class DeviceStatusHistory
{
    public int Id { get; set; }

    public int DeviceId { get; set; }

    public string? Status { get; set; }

    public DateTime? ChangedAt { get; set; }
}
