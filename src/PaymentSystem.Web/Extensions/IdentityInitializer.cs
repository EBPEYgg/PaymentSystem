using Microsoft.AspNetCore.Identity;
using PaymentSystem.Domain.Entities;
using PaymentSystem.Domain.Models;

namespace PaymentSystem.Web.Extensions
{
    public class IdentityInitializer
    {
        public static async Task InitializeRoles(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRoleUserEntity>>();
            string[] roles = { RoleConstants.Admin, RoleConstants.Merchant, RoleConstants.User };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRoleUserEntity(role));
                }
            }
        }
    }
}