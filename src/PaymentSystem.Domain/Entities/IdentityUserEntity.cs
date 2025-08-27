using Microsoft.AspNetCore.Identity;

namespace PaymentSystem.Domain.Entities
{
    public class IdentityUserEntity : IdentityUser<long>
    {
        public long? MerchantId { get; set; }

        public MerchantEntity? Merchant { get; set; }
    }
}