using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Telecom_ThesisProject.Core;
using Telecom_ThesisProject.MVVM.Model;
using Telecom_ThesisProject.Services;
using Telecom_ThesisProject.Utilities.Interfaces;

namespace Telecom_ThesisProject.MVVM.ViewModel;

public class CityAddEditViewModel : ObservableObject
{
    private readonly AddressService _addressService;
    private readonly IMessageService _messageService;

    private City? _city;

    public void OnNavigatedTo(object? parameter)
    {
        _city = parameter as City;
    }

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }
    }

    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => _errorMessage;
        set { _errorMessage = value; OnPropertyChanged(); }
    }

    public string Title => IsEdit ? "Редактирование города" : "Добавление города";

    public bool IsEdit { get; }
    public int? CityId { get; }

    public RelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }

    public Action? GoBack { get; set; }

    public CityAddEditViewModel(CityNodeViewModel? node)
    {
        _addressService = new AddressService();
        _messageService = new MessageService();

        IsEdit = node != null;
        CityId = node?.Id;
        Name = node?.Name ?? string.Empty;

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
            if (IsEdit && CityId.HasValue)
            {
                var city = new City { Id = CityId.Value, Name = Name.Trim() };
                _addressService.EditCity(city);
            }
            else
            {
                var city = new City { Name = Name.Trim() };
                _addressService.AddCity(city);
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
            errors.AppendLine("Название города обязательно.");
        else if (Name.Trim().Length > 100)
            errors.AppendLine("Название города не должно превышать 100 символов.");

        if (errors.Length > 0)
        {
            ErrorMessage = errors.ToString().Trim();
            return false;
        }

        ErrorMessage = string.Empty;
        return true;
    }
}
