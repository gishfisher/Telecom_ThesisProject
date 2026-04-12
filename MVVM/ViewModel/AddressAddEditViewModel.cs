using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Telecom_ThesisProject.Core;
using Telecom_ThesisProject.MVVM.Model;
using Telecom_ThesisProject.Services;
using Telecom_ThesisProject.Utilities.Interfaces;

namespace Telecom_ThesisProject.MVVM.ViewModel;

public class AddressAddEditViewModel : ObservableObject
{
    private readonly AddressService _addressService;
    private readonly IMessageService _messageService;

    private string _houseNumber = string.Empty;
    public string HouseNumber
    {
        get => _houseNumber;
        set { _houseNumber = value; OnPropertyChanged(); }
    }

    private string? _apartment;
    public string? Apartment
    {
        get => _apartment;
        set { _apartment = value; OnPropertyChanged(); }
    }

    private int _selectedStreetId;
    public int SelectedStreetId
    {
        get => _selectedStreetId;
        set { _selectedStreetId = value; OnPropertyChanged(); }
    }

    public ObservableCollection<Street> Streets { get; } = new();

    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => _errorMessage;
        set { _errorMessage = value; OnPropertyChanged(); }
    }

    public string Title => IsEdit ? "Редактирование дома" : "Добавление дома";

    public bool IsEdit { get; }
    public int? AddressId { get; }

    public RelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }

    public Action? GoBack { get; set; }

    public AddressAddEditViewModel(StreetNodeViewModel? parentStreet, AddressNodeViewModel? node)
    {
        _addressService = new AddressService();
        _messageService = new MessageService();

        IsEdit = node != null;
        AddressId = node?.Id;
        HouseNumber = node?.HouseNumber ?? string.Empty;
        Apartment = node?.Apartment;

        if (IsEdit && node != null)
        {
            var cities = _addressService.GetCitiesForCombo();
            foreach (var city in cities)
            {
                var streets = _addressService.GetStreetsByCityId(city.Id);
                foreach (var s in streets) Streets.Add(s);
            }
            SelectedStreetId = node.Address.StreetId;
        }
        else
        {
            if (parentStreet != null)
            {
                var city = _addressService.GetCitiesForCombo().FirstOrDefault();
                if (city != null)
                {
                    var streets = _addressService.GetStreetsByCityId(city.Id);
                    foreach (var s in streets) Streets.Add(s);
                    SelectedStreetId = parentStreet.Id;
                }
            }
        }

        SaveCommand = new RelayCommand(_ => Save());
        CancelCommand = new RelayCommand(_ => GoBack?.Invoke());
    }

    private void Save()
    {
        if (!Validate())
        {
            _messageService.Show(ErrorMessage);
            return;
        }

        try
        {
            if (IsEdit && AddressId.HasValue)
            {
                var address = new Address
                {
                    Id = AddressId.Value,
                    HouseNumber = HouseNumber.Trim(),
                    Apartment = Apartment?.Trim(),
                    StreetId = SelectedStreetId
                };
                _addressService.EditAddress(address);
            }
            else
            {
                var address = new Address
                {
                    HouseNumber = HouseNumber.Trim(),
                    Apartment = Apartment?.Trim(),
                    StreetId = SelectedStreetId
                };
                _addressService.AddAddress(address);
            }

            GoBack?.Invoke();
        }
        catch (Exception ex)
        {
            _messageService.Show(ex.Message);
        }
    }

    private bool Validate()
    {
        var errors = new StringBuilder();

        if (string.IsNullOrWhiteSpace(HouseNumber))
            errors.AppendLine("Номер дома обязателен.");
        else if (HouseNumber.Trim().Length > 10)
            errors.AppendLine("Номер дома не должен превышать 10 символов.");

        if (!string.IsNullOrWhiteSpace(Apartment) && Apartment.Trim().Length > 10)
            errors.AppendLine("Номер квартиры не должен превышать 10 символов.");

        if (SelectedStreetId <= 0)
            errors.AppendLine("Выберите улицу.");

        if (errors.Length > 0)
        {
            ErrorMessage = errors.ToString().Trim();
            return false;
        }

        ErrorMessage = string.Empty;
        return true;
    }
}
