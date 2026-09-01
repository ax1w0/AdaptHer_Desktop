using AdaptHER.Model.Models;
using AdaptHER.Model;
using AdaptHER.UI.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;

namespace AdaptHER.Class
{
    public interface IAuthorizationView
    {
        string Login { get; set; }
        string Password { get; set; }
        string ErrorMessage { get; set; }
        bool IsErrorVisible { get; set; }
        bool IsPasswordVisible { get; set; }

        event EventHandler CloseRequested;
        event EventHandler LoginAttempt;
        event EventHandler<MouseButtonEventArgs> DragMoveRequested;
    }
    public interface IAuthService
    {
        Users Authenticate(string login, string password, string captchaInput, string captchaAnswer);
        Users Register(string login, string password, string captchaInput, string captchaAnswer);
        bool ChangePassword(int userId, string oldPassword, string newPassword);
        void LogLoginAttempt(string login, bool success, string ipAddress);
    }
    public class AuthService : IAuthService
    {
        private readonly ISecurityService _security;
        private readonly ApplicationDbContext _context;
        private const int MaxFailedAttempts = 5;
        private const int LockoutMinutes = 15;

        public AuthService(ISecurityService security, ApplicationDbContext context)
        {
            _security = security;
            _context = context;
        }

        public Users Authenticate(string login, string password, string captchaInput, string captchaAnswer)
        {
            login = _security.SanitizeInput(login);

            if (!_security.ValidateCaptcha(captchaInput, captchaAnswer))
            {
                LogLoginAttempt(login, false, GetIPAddress());
                throw new UnauthorizedAccessException("Incorrect captcha");
            }

            var user = _context.Users
                .FirstOrDefault(u => u.Login == login);

            if (user == null)
            {
                LogLoginAttempt(login, false, GetIPAddress());
                throw new UnauthorizedAccessException("Incorrect login or password");
            }

            if (string.IsNullOrEmpty(user.Salt) || string.IsNullOrEmpty(user.Pepper) || string.IsNullOrEmpty(user.PasswordHash))
            {
                throw new UnauthorizedAccessException("The user data is corrupted.");
            }

            if (user.IsLocked)
            {
                if (user.LockoutEndTime != null && user.LockoutEndTime > DateTime.UtcNow)
                {
                    throw new UnauthorizedAccessException($"The account is blocked until {user.LockoutEndTime}");
                }
                else
                {
                    user.IsLocked = false;
                    user.FailedLoginAttempts = 0;
                    user.LockoutEndTime = null;
                    _context.SaveChanges();
                }
            }

            bool isValid = _security.VerifyPassword(password, user.Salt, user.Pepper, user.PasswordHash);

            if (!isValid)
            {
                user.FailedLoginAttempts++;

                if (user.FailedLoginAttempts >= MaxFailedAttempts)
                {
                    user.IsLocked = true;
                    user.LockoutEndTime = DateTime.UtcNow.AddMinutes(LockoutMinutes);
                }

                _context.SaveChanges();
                LogLoginAttempt(login, false, GetIPAddress());
                throw new UnauthorizedAccessException("Invalid login or password");
            }

            user.FailedLoginAttempts = 0;
            user.LastLoginTime = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            user.LockoutEndTime = null;
            _context.SaveChanges();

            LogLoginAttempt(login, true, GetIPAddress());
            return user;
        }

        public Users Register(string login, string password, string captchaInput, string captchaAnswer)
        {
            if (!_security.ValidateLogin(login))
                throw new ArgumentException("Invalid login");

            if (!_security.ValidatePassword(password))
                throw new ArgumentException("Password must contain at least 8 characters, uppercase and lowercase letters, numbers, and special characters");

            if (!_security.ValidateCaptcha(captchaInput, captchaAnswer))
                throw new ArgumentException("Invalid captcha");

            if (_context.Users.Any(u => u.Login == login))
                throw new ArgumentException("A user with this login already exists");

            var user = new Users
            {
                Login = login,
                Salt = _security.GenerateSalt(),
                Pepper = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash = _security.HashPassword(password, user.Salt, user.Pepper);

            //_context.Users.Add(user);
            _context.SaveChanges();

            return user;
        }

        public bool ChangePassword(int userId, string oldPassword, string newPassword)
        {
            var user = _context.Users.Find(userId);
            if (user == null) return false;

            if (!_security.VerifyPassword(oldPassword, user.Salt, user.Pepper, user.PasswordHash))
                return false;

            if (!_security.ValidatePassword(newPassword))
                return false;

            user.Salt = _security.GenerateSalt();
            user.PasswordHash = _security.HashPassword(newPassword, user.Salt, user.Pepper);
            user.UpdatedAt = DateTime.UtcNow;

            _context.SaveChanges();
            return true;
        }

        public void LogLoginAttempt(string login, bool success, string ipAddress)
        {
            try
            {
                var log = new LoginAttemptLog
                {
                    Login = login,
                    IsSuccess = success,
                    IPAddress = ipAddress,
                    AttemptTime = DateTime.UtcNow
                };

                _context.LoginAttemptLogs.Add(log);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error logging login attempt: {ex.Message}");
            }
        }

        private string GetIPAddress()
        {
            return "127.0.0.1";
        }
    }

    public class LoginAttemptLog
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public bool IsSuccess { get; set; }
        public string IPAddress { get; set; }
        public DateTime AttemptTime { get; set; }
    }
    public interface INavigationService
    {
        void NavigateToMainWindow();
        void NavigateToHRWindow();
        void CloseAuthorizationWindow();
    }
    public class NavigationService : INavigationService
    {
        public void NavigateToMainWindow()
        {
            var mainWindow = new MainWnd();
            mainWindow.Show();
        }
        public void NavigateToHRWindow()
        {
            var hrWindow = new ForHRSpecialistWnd();
            hrWindow.Show();
        }
        public void CloseAuthorizationWindow()
        {
            var authWindow = Application.Current.Windows.OfType<AuthorizationWnd>().FirstOrDefault();
            authWindow?.Close();
        }
    }
    public interface IModulesService
    {
        List<Roles> GetRoles();
        List<Modules> GetFilteredModules(int? positionId, string nameFilter);
    }
    public interface IFeedbackService
    {
        void ShowFeedback(string message);
    }
    public class ModulesService : IModulesService
    {
        public List<Roles> GetRoles()
        {
            using (var db = new ApplicationDbContext())
            {
                return db.Roles.ToList();
            }
        }
        public List<Modules> GetFilteredModules(int? positionId, string nameFilter)
        {
            using (var db = new ApplicationDbContext())
            {
                var query = db.Modules
                    .Include(x => x.Statuses)
                    .Include(x => x.Developers)
                    .ThenInclude(x => x.Develops)
                    .Include(x => x.Agreeds)
                    .ThenInclude(x => x.PersonsAgreeds)
                    .Include(x => x.ModulesPositions)
                    .ThenInclude(x => x.Positions)
                    .AsQueryable();

                if (positionId.HasValue && positionId.Value != -1)
                {
                    query = query.Where(x => x.ModulesPositions.Any(y => y.PositionId == positionId.Value));
                }

                if (!string.IsNullOrEmpty(nameFilter))
                {
                    query = query.Where(x => x.Name.Contains(nameFilter));
                }

                return query.ToList();
            }
        }
    }
    public class DefaultFeedbackService : IFeedbackService
    {
        public void ShowFeedback(string message)
        {
            MessageBox.Show(message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
    public interface INotificationService
    {
        void ShowNotification(System.Windows.Window owner, string message, int displayTime = 2000);
    }
    public class DefaultNotificationService : INotificationService
    {
        public void ShowNotification(System.Windows.Window owner, string message, int displayTime = 2000)
        {
            MessageBox.Show(message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
    public interface IModulesProgService
    {
        List<ComboBoxValueItem<int>> GetRolesForDepartment(int departmentId);
        List<ComboBoxValueItem<int>> GetModulesForRole(int roleId);
    }
    public interface IEmployeeService
    {
        List<ComboBoxValueItem<int>> SearchEmployees(string query);
        List<ComboBoxValueItem<int>> SearchMentors(string query);
    }
    public class DefaultModulesService : IModulesProgService
    {
        public List<ComboBoxValueItem<int>> GetRolesForDepartment(int departmentId)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    return db.Roles
                        .Include(x => x.RolesDepartments)
                        .Where(x => x.RolesDepartments.Any(y => y.DepartmentId == departmentId))
                        .Select(x => new ComboBoxValueItem<int>()
                        {
                            DisplayText = x.Name,
                            Value = x.Id
                        })
                        .ToList();
                }
            }
            catch (Exception)
            {
                return new List<ComboBoxValueItem<int>>();
            }
        }
        public List<ComboBoxValueItem<int>> GetModulesForRole(int roleId)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    return db.Modules
                        .Include(x => x.ModulesPositions)
                        .Where(x => x.ModulesPositions.Any(y => y.PositionId == roleId))
                        .Select(x => new ComboBoxValueItem<int>()
                        {
                            DisplayText = x.Name,
                            Value = x.Id
                        })
                        .ToList();
                }
            }
            catch (Exception)
            {
                return new List<ComboBoxValueItem<int>>();
            }
        }
    }
    public class DefaultEmployeeService : IEmployeeService
    {
        public List<ComboBoxValueItem<int>> SearchEmployees(string query)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    return db.Persons
                        .Where(x => !x.RoleId.HasValue &&
                                  (x.LastName.Contains(query) ||
                                   x.FirstName.Contains(query) ||
                                   x.Patronymic.Contains(query)))
                        .Select(x => new ComboBoxValueItem<int>()
                        {
                            DisplayText = (x.LastName + " " + x.FirstName + " " + x.Patronymic).Trim(),
                            Value = x.Id
                        })
                        .ToList();
                }
            }
            catch (Exception)
            {
                return new List<ComboBoxValueItem<int>>();
            }
        }
        public List<ComboBoxValueItem<int>> SearchMentors(string query)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    return db.Persons
                        .Where(x => x.RoleId.HasValue &&
                                  (x.LastName.Contains(query) ||
                                   x.FirstName.Contains(query) ||
                                   x.Patronymic.Contains(query)))
                        .Select(x => new ComboBoxValueItem<int>()
                        {
                            DisplayText = (x.LastName + " " + x.FirstName + " " + x.Patronymic).Trim(),
                            Value = x.Id
                        })
                        .ToList();
                }
            }
            catch (Exception)
            {
                return new List<ComboBoxValueItem<int>>();
            }
        }
    }
}
