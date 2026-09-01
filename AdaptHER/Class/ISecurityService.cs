using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace AdaptHER.Class
{
    public interface ISecurityService
    {
        string HashPassword(string password, string salt, string pepper);
        string GenerateSalt();
        bool VerifyPassword(string password, string salt, string pepper, string hash);
        string SanitizeInput(string input);
        bool ValidateLogin(string login);
        bool ValidatePassword(string password);
        string GenerateCaptcha();
        bool ValidateCaptcha(string userInput, string correctAnswer);
    }

    public class SecurityService : ISecurityService
    {
        private const int SaltSize = 32;
        private const int HashIterations = 10000;
        private const int PepperLength = 16;

        private static readonly string GlobalPepper = "MySuperSecretGlobalPepperValue123!@#";

        public string HashPassword(string password, string salt, string pepper)
        {
            string combined = password + salt + pepper + GlobalPepper;

            using (var pbkdf2 = new Rfc2898DeriveBytes(
                combined,
                Encoding.UTF8.GetBytes(salt),
                HashIterations,
                HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(32);
                return Convert.ToBase64String(hash);
            }
        }

        public string GenerateSalt()
        {
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return Convert.ToBase64String(salt);
        }

        public bool VerifyPassword(string password, string salt, string pepper, string hash)
        {
            string computedHash = HashPassword(password, salt, pepper);
            return SlowEquals(computedHash, hash);
        }

        private bool SlowEquals(string a, string b)
        {
            uint diff = (uint)a.Length ^ (uint)b.Length;
            for (int i = 0; i < a.Length && i < b.Length; i++)
            {
                diff |= (uint)(a[i] ^ b[i]);
            }
            return diff == 0;
        }

        public string SanitizeInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            string sanitized = Regex.Replace(input, @"[<>""'%;()&+]", string.Empty);

            sanitized = sanitized.Replace("'", "''");

            sanitized = sanitized.Trim();

            return sanitized;
        }

        public bool ValidateLogin(string login)
        {
            if (string.IsNullOrWhiteSpace(login) || login.Length < 3 || login.Length > 20)
                return false;

            return Regex.IsMatch(login, @"^[a-zA-Z0-9_.-]+$");
        }

        public bool ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                return false;

            bool hasUpper = Regex.IsMatch(password, @"[A-Z]");
            bool hasLower = Regex.IsMatch(password, @"[a-z]");
            bool hasDigit = Regex.IsMatch(password, @"[0-9]");
            bool hasSpecial = Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]");

            return hasUpper && hasLower && hasDigit && hasSpecial;
        }

        public string GenerateCaptcha()
        {
            Random rand = new Random();
            int num1 = rand.Next(1, 10);
            int num2 = rand.Next(1, 10);
            string[] operators = { "+", "-", "*" };
            string op = operators[rand.Next(operators.Length)];
            int result = 0;

            switch (op)
            {
                case "+":
                    result = num1 + num2;
                    break;
                case "-":
                    result = num1 - num2;
                    break;
                case "*":
                    result = num1 * num2;
                    break;
                default:
                    result = num1 + num2;
                    break;
            }

            return $"{num1} {op} {num2} = {result}";
        }

        public bool ValidateCaptcha(string userInput, string correctAnswer)
        {
            if (string.IsNullOrWhiteSpace(userInput) || string.IsNullOrWhiteSpace(correctAnswer))
                return false;

            var parts = correctAnswer.Split('=');
            if (parts.Length != 2) return false;

            string correctResult = parts[1].Trim();
            return userInput.Trim() == correctResult;
        }
    }
}
