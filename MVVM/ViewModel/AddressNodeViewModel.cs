using System;
using Telecom_ThesisProject.Core;
using Telecom_ThesisProject.MVVM.Model;

namespace Telecom_ThesisProject.MVVM.ViewModel;

public class AddressNodeViewModel : ObservableObject
{
    private readonly Address _address;

    public int Id => _address.Id;
    public string HouseNumber => _address.HouseNumber;
    public string? Apartment => _address.Apartment;
    public Address Address => _address;

    public string DisplayText => string.IsNullOrWhiteSpace(Apartment)
        ? $"д. {HouseNumber}"
        : $"д. {HouseNumber}, кв. {Apartment}";

    public Action? OpenDetails { get; set; }
    public RelayCommand DetailsCommand { get; }

    public AddressNodeViewModel(Address address)
    {
        _address = address;
        DetailsCommand = new RelayCommand(_ => OpenDetails?.Invoke());
    }
}
