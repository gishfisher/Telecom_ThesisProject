using System;
using System.Collections.Generic;

namespace Telecom_ThesisProject.MVVM.Model;

public partial class Address
{
    public int Id { get; set; }

    public int StreetId { get; set; }

    public string HouseNumber { get; set; } = null!;

    public virtual ICollection<Apartment> Apartments { get; set; } = new List<Apartment>();

    public virtual ICollection<MountingPoint> MountingPoints { get; set; } = new List<MountingPoint>();

    public virtual Street Street { get; set; } = null!;

    public string GetFullAddress =>
        string.Join(", ", new[] { Street.City.Name, Street.Name, HouseNumber }.Where(s => !string.IsNullOrWhiteSpace(s))).Trim();

    public string GetFullAddressWithApartment =>
        string.Join(", ", new[] { Street.City.Name, Street.Name, HouseNumber, Apartments.FirstOrDefault()?.Number }.Where(s => !string.IsNullOrWhiteSpace(s))).Trim();
}
