using AdaptHER.Class;
using AdaptHER.Model;
using AdaptHER.Model.Models;
using AdaptHER.UI.ViewModels;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace AdaptHER.UI.Views
{
    /// <summary>
    /// Authorization window.
    /// </summary>
    /// <remarks>
    /// The window implements access control.
    /// </remarks>

    public partial class AuthorizationWnd : Window, AdaptHER.UI.ViewModels.IAuthorizationView
    {
        private readonly AuthorizationVM _viewModel;
        public AuthorizationWnd()
        {
            InitializeComponent();

            var security = new SecurityService();
            var context = new ApplicationDbContext();
            var authService = new AuthService(security, context);
            _viewModel = new AuthorizationVM(this, authService, security);

            LoginBtn.Click += (s, e) => LoginAttempt?.Invoke(this, EventArgs.Empty);
            btnShowPassword.Checked += (s, e) => PasswordVisibilityChanged?.Invoke(this, true);
            btnShowPassword.Unchecked += (s, e) => PasswordVisibilityChanged?.Invoke(this, false);
        }
        public string EmojiCaptchaQuestion
        {
            get => EmojiCaptchaQuestionTxt.Text;
            set => EmojiCaptchaQuestionTxt.Text = value;
        }

        public string EmojiCaptchaInput
        {
            get => EmojiCaptchaInputTxt.Text;
            set => EmojiCaptchaInputTxt.Text = value;
        }
        public string CaptchaQuestion
        {
            get => CaptchaQuestionTxt.Text;
            set => CaptchaQuestionTxt.Text = value;
        }

        public string CaptchaInput
        {
            get => CaptchaInputTxt.Text;
            set => CaptchaInputTxt.Text = value;
        }

        public string Login
        {
            get => TxtLogin.Text;
            set => TxtLogin.Text = value;
        }
        public string Password
        {
            get => TxtPassword.Password;
            set => TxtPassword.Password = value;
        }
        public string ErrorMessage
        {
            get => lblError.Text;
            set => lblError.Text = value;
        }
        public bool IsErrorVisible
        {
            get => lblError.Visibility == Visibility.Visible;
            set => lblError.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        }
        public bool IsPasswordVisible
        {
            get => TxtPasswordVisible.Visibility == Visibility.Visible;
            set
            {
                if (value)
                {
                    TxtPasswordVisible.Text = TxtPassword.Password;
                    TxtPasswordVisible.Visibility = Visibility.Visible;
                    TxtPassword.Visibility = Visibility.Collapsed;
                }
                else
                {
                    TxtPassword.Password = TxtPasswordVisible.Text;
                    TxtPassword.Visibility = Visibility.Visible;
                    TxtPasswordVisible.Visibility = Visibility.Collapsed;
                }
            }
        }
        public event EventHandler CloseRequested;
        public event EventHandler LoginAttempt;
        public event EventHandler<MouseButtonEventArgs> DragMoveRequested;
        public event EventHandler<bool> PasswordVisibilityChanged;
        private void BtnShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            TxtPasswordVisible.Text = TxtPassword.Password;
            TxtPasswordVisible.Visibility = Visibility.Visible;
            TxtPassword.Visibility = Visibility.Collapsed;
            PasswordVisibilityChanged?.Invoke(this, true);
        }
        private void BtnShowPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            TxtPassword.Password = TxtPasswordVisible.Text;
            TxtPassword.Visibility = Visibility.Visible;
            TxtPasswordVisible.Visibility = Visibility.Collapsed;
            PasswordVisibilityChanged?.Invoke(this, false);
        }
        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
            Close();
        }
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMoveRequested?.Invoke(this, e);
            if (e.Handled) return;
            this.DragMove();
        }
        private void RefreshCaptcha_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CaptchaInputTxt.Text = "";
                EmojiCaptchaInputTxt.Text = "";

                var security = new SecurityService();

                string newMathCaptcha = security.GenerateCaptcha();
                string[] mathParts = newMathCaptcha.Split('=');

                CaptchaQuestionTxt.Text = $"Solve: {mathParts[0].Trim()} = ?";

                var emojiDictionary = EmojiResourceHelper.LoadEmojiDictionary();
                var random = new Random();

                int emojiCount = random.Next(3, 4);
                var allEmojis = emojiDictionary.Keys.ToList();
                var selectedEmojis = allEmojis.OrderBy(x => random.Next()).Take(emojiCount).ToList();

                int targetIndex = random.Next(selectedEmojis.Count);
                string targetEmoji = selectedEmojis[targetIndex];
                string emojiAnswer = emojiDictionary[targetEmoji];

                string emojiString = string.Join(" ", selectedEmojis);
                EmojiCaptchaQuestionTxt.Text = $"What is shown in the picture {targetEmoji}? (from the list: {emojiString})";

                lblError.Text = "";
                lblError.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating captcha: {ex.Message}");
            }
        }
    }
}
