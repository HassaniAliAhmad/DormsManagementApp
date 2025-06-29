using System;
using System.Security.Cryptography;
using System.Text;

namespace DormitoryManagement.Core.Utils
{
    public static class PasswordHasher
    {
        // In a real-world app, you'd use a library like BCrypt.Net.
        // For this project, a simple SHA256 hash is sufficient to demonstrate the concept.

        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            string hashOfInput = HashPassword(password);
            return StringComparer.OrdinalIgnoreCase.Equals(hashOfInput, hashedPassword);
        }
    }
}