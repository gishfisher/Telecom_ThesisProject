using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telecom_ThesisProject.Services
{
    class NavigationService
    {
        public Action<object> NavigateAction { get; set; }

        public void Navigate(object viewModel)
        {
            NavigateAction?.Invoke(viewModel);
        }
    }
}
