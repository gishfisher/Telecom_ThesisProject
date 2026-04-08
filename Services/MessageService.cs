using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telecom_ThesisProject.MVVM.View;
using Telecom_ThesisProject.Utilities.Interfaces;

namespace Telecom_ThesisProject.Services
{
    public class MessageService : IMessageService
    {
        public void Show(string message)
        {
            var window = new CustomMessageBoxView(message);
            window.ShowDialog();
        }
    }
}
