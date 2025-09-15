using Microsoft.AspNetCore.Identity;

namespace PaymentSystem.Domain.Entities
{
    public class IdentityRoleUserEntity : IdentityRole<long>
    {
        public IdentityRoleUserEntity() : base() { }

        public IdentityRoleUserEntity(string roleName) : base(roleName) { }
    }
}