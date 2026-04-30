using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telecom_ThesisProject.Core;
using Telecom_ThesisProject.MVVM.Model;
using Telecom_ThesisProject.Services;
using Telecom_ThesisProject.Utilities.Interfaces;

namespace Telecom_ThesisProject.MVVM.ViewModel
{
    public class ApartmentAddEditViewModel : ObservableObject
    {
        private readonly AddressService _addressService;
        private readonly IMessageService _messageService;

        #region Properties

        private string _apartmentNumber = string.Empty;
        public string ApartmentNumber
        {
            get => _apartmentNumber;
            set { _apartmentNumber = value; OnPropertyChanged(); }
        }

        private int _selectedAddressId;
        public int SelectedAddressId
        {
            get => _selectedAddressId;
            set { _selectedAddressId = value; OnPropertyChanged(); }
        }

        private int _selectedStreetId;
        public int SelectedStreetId
        {
            get => _selectedStreetId;
            set { _selectedStreetId = value; OnPropertyChanged(); }
        }

        public int _selectedCityId;
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

        public bool IsEdit { get; }
        public int? ApartmentId { get; }
        public string Title => IsEdit ? "Редактирование квартиры" : "Добавление квартиры";

        #endregion

        public ObservableCollection<Address> Addresses { get; } = new();
        public ObservableCollection<Street> Streets { get; } = new();
        public ObservableCollection<City> Cities { get; } = new();

        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }

        public Action? GoBack { get; set; }

        public ApartmentAddEditViewModel(AddressNodeViewModel? parentAddress, ApartmentNodeViewModel? node)
        {
            _addressService = new AddressService();
            _messageService = new MessageService();

            IsEdit = node != null;
            ApartmentId = node?.Id;
            ApartmentNumber = node?.ApartmentNumber ?? string.Empty;

            if (IsEdit && node != null)
            {
                SelectedCityId = node.Apartment.Address!.Street.CityId;

                var cities = _addressService.GetCitiesById(SelectedCityId)
                                            .ToList();
                foreach (var city in cities)
                {
                    Cities.Add(city);

                    var streets = _addressService.GetStreetsByCityId(city.Id)
                                                 .Where(a => a.Id == node.Apartment.Address.StreetId)
                                                 .ToList();
                    foreach (var street in streets)
                    {
                        Streets.Add(street);

                        var addresses = _addressService.GetAddressesByStreetId(street.Id)
                                                       .ToList();
                        foreach (var a in addresses)
                        {
                            Addresses.Add(a);
                        }
                    }
                    SelectedAddressId = node.Apartment.AddressId;
                }
                SelectedStreetId = node.Apartment.Address.StreetId;
            }
            else
            {
                if (parentAddress != null)
                {
                    SelectedCityId = parentAddress.Address.Street.CityId;

                    var cities = _addressService.GetCitiesById(SelectedCityId)
                                                .ToList();
                    if (cities != null)
                    {
                        foreach (var city in cities)
                        {
                            Cities.Add(city);

                            var streets = _addressService.GetStreetsByCityId(city.Id)
                                                         .Where(a => a.Id == parentAddress.Address.StreetId)
                                                         .ToList();
                            foreach (var street in streets)
                            {
                                Streets.Add(street);

                                var addresses = _addressService.GetAddressesByStreetId(street.Id)
                                                               .ToList();
                                foreach (var a in addresses)
                                {
                                    Addresses.Add(a);
                                }

                                SelectedAddressId = parentAddress.Address.Id;
                            }
                        }
                        SelectedStreetId = parentAddress.Address.StreetId;
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
                if (IsEdit && ApartmentId.HasValue)
                {
                    var address = new Apartment
                    {
                        Id = ApartmentId.Value,
                        Number = ApartmentNumber?.Trim() ?? string.Empty,
                        AddressId = SelectedAddressId
                    };
                    _addressService.EditApartment(address);
                }
                else
                {
                    var address = new Apartment
                    {
                        Number = ApartmentNumber?.Trim() ?? string.Empty, 
                        AddressId = SelectedAddressId
                    };
                    _addressService.AddApartment(address);
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

            if (!string.IsNullOrWhiteSpace(ApartmentNumber) && ApartmentNumber.Trim().Length > 10)
                errors.AppendLine("Номер квартиры не должен превышать 10 символов.");

            if (SelectedAddressId <= 0) 
                errors.AppendLine("Выберите дом.");

            if (SelectedStreetId <= 0)
                errors.AppendLine("Выберите улицу.");

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
}
