using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Telecom_ThesisProject.Data;
using Telecom_ThesisProject.MVVM.Model;

namespace Telecom_ThesisProject.Services
{
    class AddressService
    {
        public List<Address> GetAllWithLocation()
        {
            using (var db = new TelecomDbContext())
            {
                return db.Addresses
                    .Include(a => a.Street)
                    .ThenInclude(s => s.City)
                    .OrderBy(a => a.Id)
                    .ToList();
            }
        }
    }
}
