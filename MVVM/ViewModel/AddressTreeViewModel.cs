using System.Collections.ObjectModel;
using Telecom_ThesisProject.Core;
using Telecom_ThesisProject.MVVM.Model;
using Telecom_ThesisProject.Services;
using Telecom_ThesisProject.Utilities.Interfaces;

namespace Telecom_ThesisProject.MVVM.ViewModel;

public class AddressTreeViewModel : ObservableObject
{
    private readonly AddressService _addressService;
    private readonly IMessageService _messageService;

    public ObservableCollection<CityNodeViewModel> Cities { get; } = new();

    private object? _selectedNode;
    public object? SelectedNode
    {
        get => _selectedNode;
        set 
        { 
            _selectedNode = value; 
            OnPropertyChanged(); 
            //RefreshCommands(); 
        }
    }

    public RelayCommand AddCityCommand { get; }
    public RelayCommand AddStreetCommand { get; }
    public RelayCommand AddAddressCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand RefreshCommand { get; }

    public Action<CityNodeViewModel?>? NavigateAddCity { get; set; }
    public Action<CityNodeViewModel, StreetNodeViewModel?>? NavigateAddStreet { get; set; }
    public Action<StreetNodeViewModel, AddressNodeViewModel?>? NavigateAddAddress { get; set; }
    public Action<CityNodeViewModel>? NavigateEditCity { get; set; }
    public Action<StreetNodeViewModel>? NavigateEditStreet { get; set; }
    public Action<AddressNodeViewModel>? NavigateEditAddress { get; set; }
    public Action<AddressNodeViewModel>? NavigateToDetails { get; set; }

    public AddressTreeViewModel()
    {
        _addressService = new AddressService();
        _messageService = new MessageService();

        AddCityCommand = new RelayCommand(_ => NavigateAddCity?.Invoke(null));
        AddStreetCommand = new RelayCommand(_ => AddStreet(), _ => SelectedNode is CityNodeViewModel);
        AddAddressCommand = new RelayCommand(_ => AddAddress(), _ => SelectedNode is StreetNodeViewModel);
        EditCommand = new RelayCommand(_ => EditSelected(), _ => SelectedNode != null);
        DeleteCommand = new RelayCommand(_ => DeleteSelected(), _ => SelectedNode != null);
        RefreshCommand = new RelayCommand(_ => LoadTree());

        LoadTree();
    }

    private void LoadTree()
    {
        Cities.Clear();
        var cities = _addressService.GetAllCities();

        foreach (var city in cities)
        {
            var cityNode = new CityNodeViewModel(city);
            foreach (var street in city.Streets)
            {
                var streetNode = new StreetNodeViewModel(street);
                foreach (var address in street.Addresses)
                {
                    var addressNode = new AddressNodeViewModel(address);
                    addressNode.OpenDetails = () => NavigateToDetails?.Invoke(addressNode);
                    streetNode.AddAddress(addressNode);
                }
                cityNode.AddStreet(streetNode);
            }
            Cities.Add(cityNode);
        }

        OnPropertyChanged(nameof(Cities));
    }

    private void AddStreet()
    {
        if (SelectedNode is CityNodeViewModel cityNode)
        {
            NavigateAddStreet?.Invoke(cityNode, null);
        }
    }

    private void AddAddress()
    {
        if (SelectedNode is StreetNodeViewModel streetNode)
        {
            NavigateAddAddress?.Invoke(streetNode, null);
        }
    }

    private void EditSelected()
    {
        switch (SelectedNode)
        {
            case CityNodeViewModel cityNode:
                NavigateEditCity?.Invoke(cityNode);
                break;
            case StreetNodeViewModel streetNode:
                NavigateEditStreet?.Invoke(streetNode);
                break;
            case AddressNodeViewModel addressNode:
                NavigateEditAddress?.Invoke(addressNode);
                break;
        }
    }

    private void DeleteSelected()
    {
        switch (SelectedNode)
        {
            case CityNodeViewModel cityNode:
                if (_messageService.Confirm($"Удалить город \"{cityNode.Name}\" и все вложенные элементы?"))
                {
                    try
                    {
                        _addressService.RemoveCity(cityNode.City);
                        Cities.Remove(cityNode);
                    }
                    catch (Exception ex)
                    {
                        _messageService.ShowError($"Ошибка при удалении города: {ex.Message}");
                        return;
                    }
                }
                break;

            case StreetNodeViewModel streetNode:
                var parentCity = Cities.FirstOrDefault(c => c.Streets.Contains(streetNode));
                if (_messageService.Confirm($"Удалить улицу \"{streetNode.Name}\" и все дома?"))
                {
                    try
                    {
                        _addressService.RemoveStreet(streetNode.Street);
                        parentCity?.RemoveStreet(streetNode);
                    }
                    catch (Exception ex)
                    {
                        _messageService.ShowError($"Ошибка при удалении улицы: {ex.Message}");
                        return;
                    }
                }
                break;

            case AddressNodeViewModel addressNode:
                var parentStreet = Cities
                    .SelectMany(c => c.Streets)
                    .FirstOrDefault(s => s.Addresses.Contains(addressNode));
                if (_messageService.Confirm($"Удалить дом \"{addressNode.DisplayText}\"?"))
                {
                    try
                    {
                        _addressService.RemoveAddress(addressNode.Address);
                        parentStreet?.RemoveAddress(addressNode);
                    }
                    catch (Exception ex)
                    {
                        _messageService.ShowError($"Ошибка при удалении дома: {ex.Message}");
                        return;
                    }
                }
                break;
        }
    }

    //private void RefreshCommands()
    //{
    //    AddStreetCommand.RaiseCanExecuteChanged();
    //    AddAddressCommand.RaiseCanExecuteChanged();
    //    EditCommand.RaiseCanExecuteChanged();
    //    DeleteCommand.RaiseCanExecuteChanged();
    //}

    public void Refresh()
    {
        LoadTree();
    }
}
