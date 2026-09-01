using AdaptHER.Class;
using AdaptHER.Model;
using AdaptHER.Model.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AdaptHER.UI.ViewModels
{
    public class AuthorizationVM
    {
        private readonly IAuthorizationView _view;
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;
        private readonly ISecurityService _security;

        private string _mathCaptchaAnswer;
        private string _emojiCaptchaAnswer;
        private int _captchaAttempts = 0;
        private const int MaxCaptchaAttempts = 3;
        private Dictionary<string, string> _emojiDictionary;

        public AuthorizationVM(IAuthorizationView view,
                               IAuthService authService,
                               ISecurityService security,
                               INavigationService navigationService = null)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _security = security ?? throw new ArgumentNullException(nameof(security));
            _navigationService = navigationService ?? new NavigationService();
            _emojiDictionary = EmojiResourceHelper.LoadEmojiDictionary();

            _view.LoginAttempt += OnLoginAttempt;
            _view.CloseRequested += OnCloseRequested;

            GenerateNewCaptcha();
        }

        private void GenerateNewCaptcha()
        {
            _mathCaptchaAnswer = _security.GenerateCaptcha();
            _view.CaptchaQuestion = $"Solve: {_mathCaptchaAnswer.Split('=')[0].Trim()} = ?";
            GenerateEmojiCaptcha();
            _captchaAttempts = 0;
        }
        private void GenerateEmojiCaptcha()
        {
            var random = new Random();
            int emojiCount = random.Next(3, 5);

            var allEmojis = _emojiDictionary.Keys.ToList();
            var selectedEmojis = allEmojis.OrderBy(x => random.Next()).Take(emojiCount).ToList();

            int targetIndex = random.Next(selectedEmojis.Count);
            string targetEmoji = selectedEmojis[targetIndex];
            _emojiCaptchaAnswer = _emojiDictionary[targetEmoji];

            string emojiString = string.Join(" ", selectedEmojis);
            _view.EmojiCaptchaQuestion = $"What is shown in the picture {targetEmoji}? (from the list: {emojiString})";
            _view.EmojiCaptchaInput = "";
        }
        private void OnLoginAttempt(object sender, EventArgs e)
        {
            try
            {
                string login = _security.SanitizeInput(_view.Login);
                string password = _view.Password;
                string mathCaptchaInput = _view.CaptchaInput?.Trim().ToLower();
                string emojiCaptchaInput = _view.EmojiCaptchaInput?.Trim().ToLower();

                if (string.IsNullOrWhiteSpace(login) && string.IsNullOrWhiteSpace(password))
                {
                    ShowError("Please fill in the login fields");
                    return;
                }

                if (string.IsNullOrWhiteSpace(login))
                {
                    ShowError("Enter login");
                    return;
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    ShowError("Enter password");
                    return;
                }

                if (string.IsNullOrWhiteSpace(mathCaptchaInput))
                {
                    ShowError("Enter the answer to the math captcha");
                    return;
                }

                if (string.IsNullOrWhiteSpace(emojiCaptchaInput))
                {
                    ShowError("Enter the answer to the emoji captcha");
                    return;
                }

                _captchaAttempts++;
                if (_captchaAttempts > MaxCaptchaAttempts)
                {
                    GenerateNewCaptcha();
                    ShowError("Too many attempts. Please enter a new captcha");
                    return;
                }
                var mathParts = _mathCaptchaAnswer.Split('=');
                string correctMathResult = mathParts[1].Trim();

                bool isMathValid = mathCaptchaInput == correctMathResult;
                bool isEmojiValid = emojiCaptchaInput == _emojiCaptchaAnswer.ToLower();

                if (!isMathValid || !isEmojiValid)
                {
                    string errorMessage = "";
                    if (!isMathValid && !isEmojiValid)
                        errorMessage = "Invalid answer to math captcha and emoji captcha";
                    else if (!isMathValid)
                        errorMessage = "Invalid answer to math captcha";
                    else if (!isEmojiValid)
                        errorMessage = "Invalid answer to emoji captcha";

                    ShowError(errorMessage);
                    if (_captchaAttempts >= MaxCaptchaAttempts)
                    {
                        GenerateNewCaptcha();
                    }
                    return;
                }
                var user = _authService.Authenticate(login, password, mathCaptchaInput, _mathCaptchaAnswer);
                HandleSuccessfulLogin(user);
            }
            catch (UnauthorizedAccessException ex)
            {
                ShowError(ex.Message);
                if (ex.Message.Contains("captcha") || _captchaAttempts >= MaxCaptchaAttempts)
                {
                    GenerateNewCaptcha();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Connection error: {ex.Message}");
                GenerateNewCaptcha();
            }
        }
        public void RefreshEmojiCaptcha()
        {
            GenerateEmojiCaptcha();
        }

        private void HandleSuccessfulLogin(Users user)
        {
            _view.IsErrorVisible = false;

            try
            {
                using (var db = new ApplicationDbContext())
                {
                    if (user == null)
                    {
                        ShowError("Error: user not defined");
                        return;
                    }
                    var person = db.Persons
                        .FirstOrDefault(p => p.UserId == user.Id);

                    if (person != null)
                    {
                        if (person.RoleId != null && person.RoleId == 4) // HR
                        {
                            _navigationService.NavigateToHRWindow();
                        }
                        else
                        {
                            _navigationService.NavigateToMainWindow();
                        }

                        _navigationService.CloseAuthorizationWindow();
                    }
                    else
                    {
                        ShowError("No access rights information found for this user");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error retrieving data: {ex.Message}");
            }
        }

        private void ShowError(string message)
        {
            _view.ErrorMessage = message;
            _view.IsErrorVisible = true;
        }

        private void OnCloseRequested(object sender, EventArgs e)
        {
            _navigationService.CloseAuthorizationWindow();
        }
    }

    public interface IAuthorizationView
    {
        string Login { get; set; }
        string Password { get; set; }
        string ErrorMessage { get; set; }
        bool IsErrorVisible { get; set; }
        string CaptchaQuestion { get; set; }
        string CaptchaInput { get; set; }
        string EmojiCaptchaQuestion { get; set; }
        string EmojiCaptchaInput { get; set; }
        event EventHandler LoginAttempt;
        event EventHandler CloseRequested;
    }
}