using NLog;
using PaymentSystem.Application.Abstractions;
using PaymentSystem.Application.Models.Merchants;
using PaymentSystem.Domain.Data;
using PaymentSystem.Domain.Entities;

namespace PaymentSystem.Application.Services
{
    /// <summary>
    /// Сервис для работы с продавцом.
    /// </summary>
    public class MerchantsService(OrdersDbContext context) : IMerchantsService
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public async Task<MerchantsDto> Create(MerchantsDto merchant)
        {
            _logger.Debug("Received MerchantsDto: {@Merchant}.", merchant);
            var entity = new MerchantEntity()
            {
                Name = merchant.Name,
                Phone = merchant.Phone,
                WebSite = merchant.WebSite
            };

            var result = await context.Merchants.AddAsync(entity);
            var resultEntity = result.Entity;
            await context.SaveChangesAsync();
            _logger.Info("Succesfully created a merchant with id={MerchantId}.", resultEntity.Id);

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