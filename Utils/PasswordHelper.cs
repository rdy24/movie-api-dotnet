using System.Security.Cryptography;
using System.Text;

namespace MovieApp.Utils
{
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var saltedPassword = password + "MovieApp2024Salt"; // Simple salt
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
            return Convert.ToBase64String(hashedBytes);
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            var hashedInput = HashPassword(password);
            // Generate a random salt
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            using var sha256 = SHA256.Create();
            var saltedPasswordBytes = Encoding.UTF8.GetBytes(password);
            var passwordWithSaltBytes = new byte[saltedPasswordBytes.Length + salt.Length];
            Buffer.BlockCopy(saltedPasswordBytes, 0, passwordWithSaltBytes, 0, saltedPasswordBytes.Length);
            Buffer.BlockCopy(salt, 0, passwordWithSaltBytes, saltedPasswordBytes.Length, salt.Length);
            var hashedBytes = sha256.ComputeHash(passwordWithSaltBytes);
            // Store salt + hash together (salt first)
            var resultBytes = new byte[salt.Length + hashedBytes.Length];
            Buffer.BlockCopy(salt, 0, resultBytes, 0, salt.Length);
            Buffer.BlockCopy(hashedBytes, 0, resultBytes, salt.Length, hashedBytes.Length);
            return Convert.ToBase64String(resultBytes);
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            // Decode the stored value
            var hashBytes = Convert.FromBase64String(hashedPassword);
            if (hashBytes.Length < 48) // 16 bytes salt + 32 bytes SHA256 hash
                return false;
            var salt = new byte[16];
            Buffer.BlockCopy(hashBytes, 0, salt, 0, 16);
            var storedHash = new byte[32];
            Buffer.BlockCopy(hashBytes, 16, storedHash, 0, 32);
            using var sha256 = SHA256.Create();
            var saltedPasswordBytes = Encoding.UTF8.GetBytes(password);
            var passwordWithSaltBytes = new byte[saltedPasswordBytes.Length + salt.Length];
            Buffer.BlockCopy(saltedPasswordBytes, 0, passwordWithSaltBytes, 0, saltedPasswordBytes.Length);
            Buffer.BlockCopy(salt, 0, passwordWithSaltBytes, saltedPasswordBytes.Length, salt.Length);
            var computedHash = sha256.ComputeHash(passwordWithSaltBytes);
            // Compare hashes
            return CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
        }
    }
}