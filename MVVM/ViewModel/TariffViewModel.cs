using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telecom.Utilities;
using Telecom_ThesisProject.Core;
using Telecom_ThesisProject.MVVM.Model;
using Telecom_ThesisProject.Services;
using Telecom_ThesisProject.Utilities.Interfaces;

namespace Telecom_ThesisProject.MVVM.ViewModel
{
    class TariffViewModel : ObservableObject
    {
        private readonly TariffService _tariffService;
        private readonly IMessageService _messageService;

        public ObservableCollection<Tariff> Tariff { get; set; }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterTariffs();
            }
        }

        private Tariff _selectedTariff = null;
        public Tariff SelectedTariff
        {
            get => _selectedTariff;
            set { _selectedTariff = value; OnPropertyChanged(); }
        }

        public RelayCommand AddCommand { get; }
        public RelayCommand EditCommand { get; }
        public RelayCommand DeleteCommand { get; }

        public Action<Tariff> Navigate { get; set; }

        public TariffViewModel()
        {
            _tariffService = new TariffService();
            _messageService = new MessageService();

            Tariff = new ObservableCollection<Tariff>();

            LoadTariffs();
            FilterTariffs();

            AddCommand = new RelayCommand(
                o => Navigate?.Invoke(null!),
                o => !CurrentSession.IsSysAdmin);

            EditCommand = new RelayCommand(
                o => Navigate?.Invoke(SelectedTariff),
                o => SelectedTariff != null && !CurrentSession.IsSysAdmin);

            DeleteCommand = new RelayCommand(
                o => DeleteTariff(),
                o => SelectedTariff != null && !CurrentSession.IsSysAdmin);
        }

        private void LoadTariffs()
        {
            var tariffs = _tariffService.GetAll();

            Tariff.Clear();
            foreach (var tariff in tariffs)
                Tariff.Add(tariff);

            OnPropertyChanged(nameof(Tariff));
        }

        private void FilterTariffs()
        {
            var search = SearchText?.Trim() ?? string.Empty;
            var all = _tariffService.GetAll();
            var filtered = string.IsNullOrEmpty(search)
                ? all
                : all.Where(x => x.Name?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false).ToList();

            Tariff.Clear();
            foreach (var t in filtered)
                Tariff.Add(t);
        }

        private void DeleteTariff()
        {
            if (SelectedTariff == null) return;
            
            if (_messageService.Confirm($"Удалить тариф №{SelectedTariff.Id}, {SelectedTariff.Name}?"))
            {
                try
                {
                    _tariffService.RemoveTariff(SelectedTariff);
                    LoadTariffs();
                    FilterTariffs();
                }
                catch (Exception ex)
                {
                    _messageService.ShowError(ex.Message);
                }
            }
        }

        public void Refresh()
        {
            LoadTariffs();
            FilterTariffs();
        }
    }
}
