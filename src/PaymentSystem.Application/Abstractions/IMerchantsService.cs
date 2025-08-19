using PaymentSystem.Application.Models.Merchants;

namespace PaymentSystem.Application.Abstractions
{
    public interface IMerchantsService
    {
        Task<MerchantsDto> Create(MerchantsDto merchant);
    }
}