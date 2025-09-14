namespace PaymentSystem.Domain.Options
{
    public class AuthOptions
    {
        public string? Issuer { get; set; }

        public string? Audience { get; set; }

        public required string JwtTokenPrivateKey { get; set; }

        public int JwtTokenExpiryIntervalMinutes { get; set; }

        public int RefreshTokenJwtTokenExpiryIntervalMinutes { get; set; }
    }
}