using System;
using System.Collections.ObjectModel;
using Telecom_ThesisProject.Core;
using Telecom_ThesisProject.MVVM.Model;

namespace Telecom_ThesisProject.MVVM.ViewModel;

public class AddressNodeViewModel : ObservableObject
{
    public Address Address => _address;
    private readonly Address _address;
    
    public ObservableCollection<ApartmentNodeViewModel> Apartments { get; }

    public int? Id => _address.Id;
    public string HouseNumber => _address.HouseNumber;
    public string DisplayText => $"д. {HouseNumber}";

    public Action? OpenDetails { get; set; }
    public RelayCommand DetailsCommand { get; }

    public AddressNodeViewModel(Address address)
    {
        _address = address;
        Apartments = new ObservableCollection<ApartmentNodeViewModel>();
        DetailsCommand = new RelayCommand(_ => OpenDetails?.Invoke());
    }

    public void AddApartment(ApartmentNodeViewModel apartment)
    {
        Apartments.Add(apartment);
    }

    public void RemoveApartment(ApartmentNodeViewModel apartment)
    {
        Apartments.Remove(apartment);
    }
}
