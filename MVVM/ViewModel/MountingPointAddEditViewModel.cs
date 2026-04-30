using System;
using System.Collections.ObjectModel;
using System.Text;
using Telecom_ThesisProject.Core;
using Telecom_ThesisProject.MVVM.Model;
using Telecom_ThesisProject.Services;
using Telecom_ThesisProject.Utilities.Interfaces;

namespace Telecom_ThesisProject.MVVM.ViewModel;

public class MountingPointAddEditViewModel : ObservableObject
{
    private readonly AddressService _addressService;
    private readonly IMessageService _messageService;

    public MountingPoint MountingPoint { get; }

    private string? _locationDescription;
    public string? LocationDescription
    {
        get => _locationDescription;
        set { _locationDescription = value; OnPropertyChanged(); }
    }

    private int _selectedPointTypeId;
    public int SelectedPointTypeId
    {
        get => _selectedPointTypeId;
        set { _selectedPointTypeId = value; OnPropertyChanged(); }
    }

    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => _errorMessage;
        set { _errorMessage = value; OnPropertyChanged(); }
    }

    public ObservableCollection<MountingPointType> PointTypes { get; } = new();
    public ObservableCollection<MountingPoint> MountingPoints { get; } = new();

    public string Title => IsEdit ? "Редактирование точки монтажа" : "Добавление точки монтажа";
    public bool IsEdit { get; }

    public RelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }

    public Action? GoBack { get; set; }

    public MountingPointAddEditViewModel(int addressId, MountingPoint mp)
    {
        _addressService = new AddressService();
        _messageService = new MessageService();

        IsEdit = mp != null;

        var sourceMp = mp?.Id > 0
           ? _addressService.GetMountingPointsById(mp.Id) ?? mp
           : mp;

        MountingPoint = CreateEditablePoint(sourceMp, addressId);

        var types = _addressService.GetMountingPointTypes();
        foreach (var t in types) PointTypes.Add(t);

        if (IsEdit && sourceMp != null)
        {
            SelectedPointTypeId = sourceMp.PointTypeId;
            LocationDescription = sourceMp.LocationDescription;
        }
        else if (types.Count > 0)
        {
            SelectedPointTypeId = types[0].Id;
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
            MountingPoint.PointTypeId = SelectedPointTypeId;
            MountingPoint.LocationDescription = LocationDescription;

            if (IsEdit && MountingPoint.Id > 0)
            {
                _addressService.EditMountingPoint(MountingPoint);
            }
            else
            {
                _addressService.AddMountingPoint(MountingPoint);
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

        if (SelectedPointTypeId <= 0)
            errors.AppendLine("Выберите тип точки монтажа.");

        if (string.IsNullOrWhiteSpace(LocationDescription))
            errors.AppendLine("Введите описание расположения.");
        else if (LocationDescription.Length > 255)
            errors.AppendLine("Описание не может быть длиннее 255 символов.");

        if (errors.Length > 0)
        {
            ErrorMessage = errors.ToString().Trim();
            return false;
        }

        ErrorMessage = string.Empty;
        return true;
    }

    private static MountingPoint CreateEditablePoint(MountingPoint? mountingPoint, int addressId)
    {
        if (mountingPoint == null)
        {
            return new MountingPoint
            {
                AddressId = addressId,
                PointTypeId = 0,
                LocationDescription = string.Empty,
                MountingDate = DateOnly.FromDateTime(DateTime.Today)
            };
        }

        return new MountingPoint
        {
            Id = mountingPoint.Id,
            AddressId = mountingPoint.AddressId,
            PointTypeId = mountingPoint.PointTypeId,
            LocationDescription = mountingPoint.LocationDescription,
            MountingDate = mountingPoint.MountingDate
        };
    }
}