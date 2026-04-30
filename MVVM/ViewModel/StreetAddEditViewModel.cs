using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Telecom_ThesisProject.Core;
using Telecom_ThesisProject.MVVM.Model;
using Telecom_ThesisProject.Services;
using Telecom_ThesisProject.Utilities.Interfaces;

namespace Telecom_ThesisProject.MVVM.ViewModel;

public class StreetAddEditViewModel : ObservableObject
{
    private readonly AddressService _addressService;
    private readonly IMessageService _messageService;

    #region Properties

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }
    }

    private int _selectedCityId;
    public int SelectedCityId
    {
        get => _selectedCityId;
        set { _selectedCityId = value; OnPropertyChanged(); }
    }

    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => _errorMessage;
        set { _errorMessage = value; OnPropertyChanged(); }
    }

    public string Title => IsEdit ? "Редактирование улицы" : "Добавление улицы";

    public bool IsEdit { get; }
    public int? StreetId { get; }

    #endregion

    public ObservableCollection<City> Cities { get; } = new();

    public RelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }

    public Action? GoBack { get; set; }

    public StreetAddEditViewModel(CityNodeViewModel? parentCity, StreetNodeViewModel? node)
    {
        _addressService = new AddressService();
        _messageService = new MessageService();

        IsEdit = node != null;
        StreetId = node?.Id;
        Name = node?.Name ?? string.Empty;

        var cities = _addressService.GetCitiesForCombo();

        foreach (var c in cities) 
            Cities.Add(c);

        if (IsEdit && node != null)
        {
            SelectedCityId = node.Street.CityId;
        }
        else
        {
            SelectedCityId = parentCity?.Id ?? cities.FirstOrDefault()?.Id ?? 0;
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
            if (IsEdit && StreetId.HasValue)
            {
                var street = new Street 
                { 
                    Id = StreetId.Value, 
                    Name = Name.Trim(), 
                    CityId = SelectedCityId 
                };
                _addressService.EditStreet(street);
            }
            else
            {
                var street = new Street 
                { 
                    Name = Name.Trim(), 
                    CityId = SelectedCityId 
                };
                _addressService.AddStreet(street);
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

        if (string.IsNullOrWhiteSpace(Name))
            errors.AppendLine("Название улицы обязательно.");
        else if (Name.Trim().Length > 100)
            errors.AppendLine("Название улицы не должно превышать 100 символов.");

        if (SelectedCityId <= 0)
            errors.AppendLine("Выберите город.");

        if (errors.Length > 0)
        {
            ErrorMessage = errors.ToString().Trim();
            return false;
        }

        ErrorMessage = string.Empty;
        return true;
    }
}