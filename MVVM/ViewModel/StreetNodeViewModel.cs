using System.Collections.ObjectModel;
using Telecom_ThesisProject.Core;
using Telecom_ThesisProject.MVVM.Model;

namespace Telecom_ThesisProject.MVVM.ViewModel;

public class StreetNodeViewModel : ObservableObject
{
    private readonly Street _street;
    public Street Street => _street;
    
    public int? Id => _street.Id;
    public string Name => _street.Name;

    public ObservableCollection<AddressNodeViewModel> Addresses { get; }
    
    public StreetNodeViewModel(Street street)
    {
        _street = street;
        Addresses = new ObservableCollection<AddressNodeViewModel>();
    }

    public void AddAddress(AddressNodeViewModel address)
    {
        Addresses.Add(address);
    }

    public void RemoveAddress(AddressNodeViewModel address)
    {
        Addresses.Remove(address);
    }
}
