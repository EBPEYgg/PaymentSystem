using PaymentSystem.Application.Models.Carts;

namespace PaymentSystem.Application.Abstractions
{
    public interface ICartsService
    {
        Task<CartDto> Create(CartDto cart);
    }
}