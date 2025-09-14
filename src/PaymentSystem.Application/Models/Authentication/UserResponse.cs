namespace PaymentSystem.Application.Models.Authentication
{
    public class UserResponse
    {
        public long Id { get; set; }

        public string[]? Roles { get; set; }

        public string? Email { get; set; }

        public string? Username { get; set; }

        public string? Phone { get; set; }

        public string? JwtToken { get; set; }

        public string? RefreshToken { get; internal set; }

        public bool IsLoggedIn { get; set; } = false;
    }
}