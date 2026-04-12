using System.Collections.ObjectModel;
using Telecom_ThesisProject.Core;
using Telecom_ThesisProject.MVVM.Model;

namespace Telecom_ThesisProject.MVVM.ViewModel;

public class CityNodeViewModel : ObservableObject
{
    private readonly City _city;
    private bool _isExpanded;

    public CityNodeViewModel(City city)
    {
        _city = city;
        Streets = new ObservableCollection<StreetNodeViewModel>();
    }

    public int Id => _city.Id;
    public string Name => _city.Name;
    public City City => _city;

    public ObservableCollection<StreetNodeViewModel> Streets { get; }

    public bool IsExpanded
    {
        get => _isExpanded;
        set { _isExpanded = value; OnPropertyChanged(); }
    }

    public void AddStreet(StreetNodeViewModel street)
    {
        Streets.Add(street);
    }

    public void RemoveStreet(StreetNodeViewModel street)
    {
        Streets.Remove(street);
    }
}
