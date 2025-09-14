using Microsoft.AspNetCore.Identity;

namespace PaymentSystem.Domain.Entities
{
    public class IdentityUserEntity : IdentityUser<long>
    {
        public long? MerchantId { get; set; }

        public MerchantEntity? Merchant { get; set; }

        public string? RefreshToken { get; set; }

        public DateTime RefreshTokenExpiry { get; set; }
    }
}