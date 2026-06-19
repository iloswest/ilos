using System;
using System.Security.Cryptography;
using System.Text;

namespace App.Shared.Helpers
{
    public static class PasswordHelper
    {
        /// <summary>
        /// Хеширует пароль с использованием SHA256
        /// </summary>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return string.Empty;

            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hashBytes = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hashBytes).ToLower();
        }

        /// <summary>
        /// Проверяет, соответствует ли пароль хешу
        /// </summary>
        public static bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
                return false;

            var passwordHash = HashPassword(password);
            return passwordHash.Equals(hash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
