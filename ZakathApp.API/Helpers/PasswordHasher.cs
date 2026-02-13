using BCrypt.Net;

namespace ZakathApp.API.Helpers
{
    public class PasswordHasher
    {
        public string HashPassword(string password)
        {
            // Generate salt and hash password
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch
            {
                return false;
            }
        }

        public string GenerateSalt()
        {
            return BCrypt.Net.BCrypt.GenerateSalt(12);
        }
    }
}
