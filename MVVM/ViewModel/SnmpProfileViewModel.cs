using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Xml.Linq;
using Telecom_ThesisProject.Core;
using Telecom_ThesisProject.MVVM.Model;
using Telecom_ThesisProject.Services;
using Telecom_ThesisProject.Utilities.Interfaces;

namespace Telecom_ThesisProject.MVVM.ViewModel
{
    public enum SnmpVersion
    {
        V1 = 1,
        V2c = 2,
        V3 = 3
    }

    public sealed class SnmpVersionItem
    {
        public int Id { get; init; }
        public string? VersionName { get; init; }
    }

    class SnmpProfileViewModel : ObservableObject
    {
        private readonly NetworkDeviceService _networkDeviceService;
        private readonly IMessageService _messageService;

        public SnmpProfile SnmpProfile { get; }

        private string _name = string.Empty;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        private string _communityString = string.Empty;

        public string CommunityString
        {
            get => _communityString;
            set { _communityString = value; OnPropertyChanged(); }
        }

        private int _port;

        public int Port
        {
            get => _port;
            set { _port = value; OnPropertyChanged(); }
        }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public bool IsEdit { get; }
        public int ProfileId { get; } = 0;

        public ObservableCollection<SnmpVersionItem> SnmpVersions { get; } = new();

        private int _selectedSnmpVersionId;
        public int SelectedSnmpVersionId
        {
            get => _selectedSnmpVersionId;
            set { if (_selectedSnmpVersionId != value) { _selectedSnmpVersionId = value; OnPropertyChanged(nameof(SelectedSnmpVersionId)); } }
        }

        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }

        public Action? GoBack { get; set; }

        public SnmpProfileViewModel(SnmpProfile profile)
        {
            _networkDeviceService = new NetworkDeviceService();
            _messageService = new MessageService();

            foreach (SnmpVersion v in Enum.GetValues(typeof(SnmpVersion)))
            {
                SnmpVersions.Add(new SnmpVersionItem
                {
                    Id = (int)v,
                    VersionName = v switch
                    {
                        SnmpVersion.V1 => "SNMP v1",
                        SnmpVersion.V2c => "SNMP v2c",
                        SnmpVersion.V3 => "SNMP v3",
                        _ => v.ToString()
                    }
                });
            }

            var sourceProfile = profile;
            if (profile?.Id > 0)
                sourceProfile = _networkDeviceService.GetSnmpProfilesById(profile.Id).FirstOrDefault() ?? profile;

            SnmpProfile = sourceProfile;

            IsEdit = SnmpProfile != null;
            ProfileId = SnmpProfile?.Id ?? 0;
            SelectedSnmpVersionId = (int)SnmpVersion.V2c;

            if (IsEdit && SnmpProfile != null)
            {
                if (int.TryParse(SnmpProfile.Version, out int parsedId))
                {
                    SelectedSnmpVersionId = parsedId;
                }
                else if (Enum.TryParse<SnmpVersion>(SnmpProfile.Version, true, out var enumVal))
                {
                    SelectedSnmpVersionId = (int)enumVal;
                }
                else
                {
                    SelectedSnmpVersionId = (int)SnmpVersion.V2c;
                }

                Name = SnmpProfile.Name;
                Port = SnmpProfile.Port;
                CommunityString = SnmpProfile.Community;
            }
            else if (ProfileId == 0)
            {
                Name = string.Empty;
                Port = 0;
                CommunityString = "public";
                SelectedSnmpVersionId = (int)SnmpVersion.V2c;
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
                if (IsEdit)
                {
                    var profile = new SnmpProfile
                    {
                        Id = ProfileId,
                        Name = Name.Trim(),
                        Version = SelectedSnmpVersionId.ToString(),
                        Port = Port,
                        Community = CommunityString.Trim()
                    };
                    _networkDeviceService.EditSnmpProfile(profile);

                    _messageService.Show("Профиль SNMP успешно обновлен.");

                    GoBack?.Invoke();
                }
                else
                {
                    var profile = new SnmpProfile
                    {
                        Id = ProfileId,
                        Name = Name.Trim(),
                        Version = SelectedSnmpVersionId.ToString(),
                        Port = Port,
                        Community = CommunityString.Trim()
                    };
                    _networkDeviceService.AddSnmpProfile(profile);

                    _messageService.Show("Профиль SNMP успешно добавлен.");

                    GoBack?.Invoke();
                }
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
                errors.AppendLine("Имя профиля SNMP обязательно.");
            else if (Name.Trim().Length > 50)
                errors.AppendLine("Имя профиля SNMP не должно превышать 50 символов.");

            if (string.IsNullOrWhiteSpace(CommunityString))
                errors.AppendLine("CommunityString профиля SNMP обязательно.");
            else if (CommunityString.Trim().Length > 50)
                errors.AppendLine("CommunityString профиля SNMP не должно превышать 50 символов.");

            if (Port <= 0)
                errors.AppendLine("Порт профиля SNMP должен быть больше 0.");

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
