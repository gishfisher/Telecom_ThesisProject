using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telecom_ThesisProject.MVVM.Model;

namespace Telecom_ThesisProject.Utilities.DTOs
{
    public class NetworkDeviceDto
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public int DeviceTypeId { get; set; }

        public string IpAddress { get; set; } = null!;

        public int? SnmpProfileId { get; set; }

        public int? MountingPointId { get; set; }

        public int? ParentDeviceId { get; set; }

        public DateOnly? InstallationDate { get; set; }

        public virtual DeviceType DeviceType { get; set; } = null!;

        public virtual MountingPoint? MountingPoint { get; set; }
    }
}
