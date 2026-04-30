using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telecom_ThesisProject.Core;
using Telecom_ThesisProject.MVVM.Model;

namespace Telecom_ThesisProject.Utilities.DTOs
{

    /// <summary>
    /// Временный объект для передачи данных о сетевом устройстве и его портах.
    /// </summary>
    public class DevicePortDto : ObservableObject
    {
        public int Id { get; set; }
        public int DeviceId { get; set; }
        public string PortName { get; set; } = null!;
        
        private bool _isUplink;
        public bool IsUplink
        {
            get => _isUplink;
            set
            {
                if (_isUplink != value)
                {
                    _isUplink = value;
                    OnPropertyChanged(nameof(IsUplink));
                }
            }
        }

        private string _operationalStatus = "Неизвестно";
        public string OperationalStatus
        {
            get => _operationalStatus;
            set
            {
                _operationalStatus = value;
                OnPropertyChanged();
            }
        }
        public int StatusRawValue { get; set; }
        public bool? HasConnection { get; set; }
        public Connection? Connection { get; set; }
    }
}
