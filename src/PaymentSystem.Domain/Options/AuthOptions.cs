namespace PaymentSystem.Domain.Options
{
    public class AuthOptions
    {
        public string? Issuer { get; set; }

        public string? Audience { get; set; }

        public required string TokenPrivateKey { get; set; }

        public int ExpireIntervalMinutes { get; set; }
    }
}