using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telecom_ThesisProject.Data;
using Telecom_ThesisProject.MVVM.Model;
using TelecomCompany.ApplicationData.Crypt;

namespace Telecom_ThesisProject.Services
{
    class EmployeeService
    {
        public void RegisterEmployee(Employee employee, string password)
        {
            if (employee == null || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException(nameof(employee));
            }

            if (employee.User == null)
            {
                throw new ArgumentException("Необходимо указать данные о сотруднике.", nameof(employee));
            }

            if (string.IsNullOrWhiteSpace(employee.User.Login))
            {
                throw new ArgumentException("Логин пользователя не может быть пустым.", nameof(employee));
            }

            using (var db = new TelecomDbContext())
            {
                if (db.Users.Any(u => u.Login == employee.User.Login))
                {
                    throw new Exception("Пользователь с таким логином уже существует");
                }

                employee.User.PasswordHash =
                    MD5Hasher.HashPassword(password);

                employee.User.CreatedAt = DateTime.Now;

                db.Employees.Add(employee);
                db.SaveChanges();
            }
        }

        public void EditEmployee(Employee employee, string? newPassword)
        {
            ArgumentNullException.ThrowIfNull(employee);
            if (employee.User == null)
            {
                throw new ArgumentException("Необходимо указать данные о сотруднике.", nameof(employee));
            }

            if (string.IsNullOrWhiteSpace(employee.User.Login))
            {
                throw new ArgumentException("Логин пользователя не может быть пустым.", nameof(employee));
            }

            using (var db = new TelecomDbContext())
            {
                var existingEmployee = db.Employees
                    .Include(e => e.User)
                    .FirstOrDefault(e => e.Id == employee.Id) ?? throw new Exception("Сотрудник не найден");

                var existingUser = existingEmployee.User ?? throw new Exception("У сотрудника отсутствует связанный пользователь");

                if (db.Users.Any(u =>
                    u.Login == employee.User.Login &&
                    u.Id != existingUser.Id))
                {
                    throw new Exception("Логин уже занят");
                }

                existingEmployee.FirstName = employee.FirstName;
                existingEmployee.LastName = employee.LastName;
                existingEmployee.MiddleName = employee.MiddleName;

                existingUser.IsActive = employee.User.IsActive;
                existingUser.Login = employee.User.Login;
                existingUser.RoleId = employee.User.RoleId;

                if (!string.IsNullOrWhiteSpace(newPassword))
                {
                    existingUser.PasswordHash =
                        MD5Hasher.HashPassword(newPassword);
                }

                db.SaveChanges();
            }
        }

        public void DeleteEmployee(Employee employee)
        {
            ArgumentNullException.ThrowIfNull(employee);

            using (var db = new TelecomDbContext())
            {
                var employeeForDelete = db.Employees
                    .Include(e => e.User)
                    .FirstOrDefault(e => e.Id == employee.Id) ?? throw new Exception("Сотрудник не найден");

                db.Employees.Remove(employeeForDelete);

                if (employeeForDelete.User != null)
                {
                    db.Users.Remove(employeeForDelete.User);
                }

                db.SaveChanges();
            }
        }

        public List<Employee> GetAll()
        {
            using (var db = new TelecomDbContext())
            {
                return db.Employees
                    .Include(e => e.User)
                    .ThenInclude(u => u.Role)
                    .ToList();
            }
        }

        public List<Role> GetAllRoles()
        {
            using (var db = new TelecomDbContext())
            {
                return db.Roles.ToList();
            }
        }
    }
}