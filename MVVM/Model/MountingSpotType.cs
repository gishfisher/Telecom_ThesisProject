using System;
using System.Collections.Generic;

namespace Telecom_ThesisProject.MVVM.Model;

public partial class MountingSpotType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<MountingPoint> MountingPoints { get; set; } = new List<MountingPoint>();
}
