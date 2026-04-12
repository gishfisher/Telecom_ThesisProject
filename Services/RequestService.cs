using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Telecom_ThesisProject.Data;
using Telecom_ThesisProject.MVVM.Model;

namespace Telecom_ThesisProject.Services
{
    class RequestService
    {
        public void AddRequest(Request request)
        {
            ArgumentNullException.ThrowIfNull(request);
            DetachNavigations(request);

            using (var db = new TelecomDbContext())
            {
                request.CreatedAt = DateTime.Now;
                if (request.StatusId == null || request.StatusId == 0)
                    request.StatusId = 1;

                db.Requests.Add(request);
                db.SaveChanges();
            }
        }

        public void EditRequest(Request request)
        {
            ArgumentNullException.ThrowIfNull(request);
            DetachNavigations(request);

            using (var db = new TelecomDbContext())
            {
                var existingRequest = db.Requests
                    .FirstOrDefault(r => r.Id == request.Id) ?? throw new Exception("Заявка не найдена");

                existingRequest.Description = request.Description;
                existingRequest.ClientId = request.ClientId;
                existingRequest.EmployeeId = request.EmployeeId;
                existingRequest.DeviceId = request.DeviceId;
                existingRequest.StatusId = request.StatusId;

                db.SaveChanges();
            }
        }

        public void RemoveRequest(Request request)
        {
            ArgumentNullException.ThrowIfNull(request);

            using (var db = new TelecomDbContext())
            {
                var existingRequest = db.Requests
                    .FirstOrDefault(r => r.Id == request.Id) ?? throw new Exception("Заявка не найдена");

                db.Requests.Remove(existingRequest);
                db.SaveChanges();
            }
        }

        public List<Request> GetAll()
        {
            using (var db = new TelecomDbContext())
            {
                return db.Requests
                    .Include(r => r.Client)
                    .Include(r => r.Employee)
                    .Include(r => r.Status)
                    .ToList();
            }
        }

        private static void DetachNavigations(Request request)
        {
            request.Client = null;
            request.Employee = null;
            request.Device = null;
            request.Status = null;
        }
    }
}
