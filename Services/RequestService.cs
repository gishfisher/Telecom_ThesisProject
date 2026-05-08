using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using Telecom_ThesisProject.Data;
using Telecom_ThesisProject.MVVM.Model;

namespace Telecom_ThesisProject.Services
{
    class RequestService
    {
        public void AddRequest(MVVM.Model.Request request)
        {
            ArgumentNullException.ThrowIfNull(request);

            using (var db = new TelecomDbContext())
            {
                request.CreatedAt = DateTime.Now;
                if (request.StatusId == null || request.StatusId == 0)
                    request.StatusId = 1;

                db.Requests.Add(request);
                db.SaveChanges();
            }
        }

        public void EditRequest(MVVM.Model.Request request)
        {
            ArgumentNullException.ThrowIfNull(request);

            using (var db = new TelecomDbContext())
            {
                var existingRequest = db.Requests
                    .FirstOrDefault(r => r.Id == request.Id) ?? throw new Exception("Заявка не найдена");

                existingRequest.Description = request.Description ?? existingRequest.Description;
                existingRequest.ClientId = request.ClientId == 0 ? existingRequest.ClientId : request.ClientId;
                existingRequest.EmployeeId = request.EmployeeId ?? existingRequest.EmployeeId;
                existingRequest.StatusId = request.StatusId ?? existingRequest.StatusId;

                db.SaveChanges();
            }
        }

        public void RemoveRequest(MVVM.Model.Request request)
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

        // === Getters === 

        public List<MVVM.Model.Request> GetAll()
        {
            using (var db = new TelecomDbContext())
            {
                return db.Requests
                    .Include(r => r.Client)
                    .Include(r => r.Employee)
                    .Include(r => r.Type)
                    .Include(r => r.Status)
                    .ToList();
            }
        }

        public List<RequestStatus> GetAllStatuses()
        {
            using (var db = new TelecomDbContext())
            {
                return db.RequestStatuses.ToList();
            }
        }

        public List<RequestsType> GetAllRequestTypes()
        {
            using (var db = new TelecomDbContext())
            {
                return db.RequestsTypes.ToList();
            }
        }

        public MVVM.Model.Request GetRequestById(int id)
        {
            using (var db = new TelecomDbContext())
            {
                return db.Requests
                    .Include(r => r.Client)
                    .Include(r => r.Employee)
                    .Include(r => r.Type)
                    .Include(r => r.Status)
                    .FirstOrDefault(r => r.Id == id)!;
            }
        }

        // === RequestComments ===

        public void AddComment(RequestComment requestComment)
        {
            ArgumentNullException.ThrowIfNull(requestComment);

            using (var db = new TelecomDbContext())
            {
                db.RequestComments.Add(requestComment);
                db.SaveChanges();
            }
        }

        public void EditComment(RequestComment requestComment)
        {
            ArgumentNullException.ThrowIfNull(requestComment);

            using (var db = new TelecomDbContext())
            {
                var existingComment = db.RequestComments
                    .FirstOrDefault(r => r.Id == requestComment.Id) ?? throw new Exception("Комментарий не найден");

                existingComment.Comment = requestComment.Comment;
                existingComment.RequestId = requestComment.RequestId;
                existingComment.CreatedAt = requestComment.CreatedAt;
                existingComment.CreatedBy = requestComment.CreatedBy;

                db.SaveChanges();
            }
        }

        public void RemoveComment(RequestComment requestComment)
        {
            ArgumentNullException.ThrowIfNull(requestComment);

            using (var db = new TelecomDbContext())
            {
                var existingComment = db.RequestComments
                   .FirstOrDefault(r => r.Id == requestComment.Id) ?? throw new Exception("Комментарий не найден");

                db.RequestComments.Remove(existingComment);
                db.SaveChanges();
            }
        }

        // === Getters ===

        public List<RequestComment> GetRequestComments(int requestId)
        {
            using (var db = new TelecomDbContext())
            {
                return db.RequestComments
                    .AsNoTracking()
                    .Include(c => c.CreatedByNavigation)
                        .ThenInclude(c => c.User)
                            .ThenInclude(c => c.Role)
                    .Where(r => r.RequestId == requestId).ToList();
            }
        }

        public RequestComment? GetCommentById(int commentId)
        {
            using (var db = new TelecomDbContext())
            {
                return db.RequestComments
                    .AsNoTracking()
                    .Include(c => c.CreatedByNavigation)
                        .ThenInclude(c => c.User)
                            .ThenInclude(c => c.Role)
                    .FirstOrDefault(r => r.Id == commentId);
            }
        }
    }
}
