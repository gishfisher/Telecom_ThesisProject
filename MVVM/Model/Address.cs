using System;
using System.Collections.Generic;

namespace Telecom_ThesisProject.MVVM.Model;

public partial class Address
{
    public int Id { get; set; }

    public int StreetId { get; set; }

    public string HouseNumber { get; set; } = null!;

    public string? Apartment { get; set; }

    public virtual ICollection<Client> Clients { get; set; } = new List<Client>();

    public virtual ICollection<MountingPoint> MountingPoints { get; set; } = new List<MountingPoint>();

    public virtual Street Street { get; set; } = null!;

    /// <summary>Строка для ComboBox (нужны загруженные Street, City).</summary>
    public string DisplayLine
    {
        get
        {
            if (Street == null)
                return $"Адрес #{Id}";

            var city = Street.City?.Name ?? "";
            var apt = string.IsNullOrWhiteSpace(Apartment) ? "" : $", кв. {Apartment}";
            return $"{city}, {Street.Name}, д. {HouseNumber}{apt}";
        }
    }
}
