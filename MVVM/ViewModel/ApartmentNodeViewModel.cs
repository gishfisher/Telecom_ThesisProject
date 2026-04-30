using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telecom_ThesisProject.Core;
using Telecom_ThesisProject.MVVM.Model;

namespace Telecom_ThesisProject.MVVM.ViewModel
{
    public class ApartmentNodeViewModel : ObservableObject
    {
        private readonly Apartment _apartment;
        public Apartment Apartment => _apartment;

        public int? Id => _apartment.Id;
        public string ApartmentNumber => _apartment?.Number ?? "0";
        public string DisplayText => $"кв. {ApartmentNumber}";

        public ApartmentNodeViewModel(Apartment apartment)
        {
           _apartment = apartment;
        }
    }
}
