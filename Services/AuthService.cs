using BCrypt.Net;

namespace FinanceManager.WinForms.Services
{
    public class AuthService
    {
        public string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);
        public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
