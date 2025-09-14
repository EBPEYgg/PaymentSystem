namespace PaymentSystem.Application.Models.Authentication
{
    public class RefreshTokenModel
    {
        public string JwtToken { get; set; }

        public string RefreshToken { get; set; }
    }
}