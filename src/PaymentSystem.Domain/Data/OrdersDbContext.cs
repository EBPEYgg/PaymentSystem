using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PaymentSystem.Domain.Entities;

namespace PaymentSystem.Domain.Data
{
    public sealed class OrdersDbContext : IdentityDbContext<IdentityUserEntity, IdentityRoleUserEntity, long>
    {
        public DbSet<CustomerEntity> Customers { get; set; } = null!;

        public DbSet<CartEntity> Carts { get; set; } = null!;

        public DbSet<CartItemEntity> CartItems { get; set; } = null!;

        public DbSet<OrderEntity> Orders { get; set; } = null!;

        public DbSet<MerchantEntity> Merchants { get; set; } = null!;

        public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options)
        {
            if (Database.GetPendingMigrations().Any())
            {
                Database.Migrate();
            }
        }
    }
}