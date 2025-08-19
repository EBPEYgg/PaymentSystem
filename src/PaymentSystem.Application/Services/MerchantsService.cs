using PaymentSystem.Application.Abstractions;
using PaymentSystem.Application.Models.Merchants;
using PaymentSystem.Domain.Data;
using PaymentSystem.Domain.Entities;

namespace PaymentSystem.Application.Services
{
    public class MerchantsService(OrdersDbContext context) : IMerchantsService
    {
        public async Task<MerchantsDto> Create(MerchantsDto merchant)
        {
            var entity = new MerchantEntity()
            {
                Name = merchant.Name,
                Phone = merchant.Phone,
                WebSite = merchant.WebSite
            };

            var result = await context.Merchants.AddAsync(entity);
            var resultEntity = result.Entity;
            await context.SaveChangesAsync();

            return new MerchantsDto()
            {
                Name = resultEntity.Name,
                Phone = resultEntity.Phone,
                WebSite = resultEntity.WebSite,
                Id = resultEntity.Id
            };
        }
    }
}