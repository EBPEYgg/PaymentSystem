using Microsoft.EntityFrameworkCore;
using NLog;
using PaymentSystem.Application.Abstractions;
using PaymentSystem.Application.Mappers;
using PaymentSystem.Application.Models.Carts;
using PaymentSystem.Domain.Data;
using PaymentSystem.Domain.Entities;

namespace PaymentSystem.Application.Services
{
    /// <summary>
    /// Сервис для работы с корзиной.
    /// </summary>
    public class CartsService(OrdersDbContext context) : ICartsService
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public async Task<CartDto> Create(CartDto cart)
        {
            _logger.Debug("Received CartDto: {@Cart}", cart);
            var cartEntity = new CartEntity();
            var cartSaveResult = await context.Carts.AddAsync(cartEntity);
            await context.SaveChangesAsync();
            _logger.Info("Created new empty Cart with Id={CartId}", cartSaveResult.Entity.Id);

            var cartItems = cart.CartItems
                .Select(item => new CartItemEntity
                {
                    Name = item.Name,
                    Price = item.Price,
                    Quantity = item.Quantity,
                    CartId = cartSaveResult.Entity.Id
                });

            await context.CartItems.AddRangeAsync(cartItems);
            await context.SaveChangesAsync();
            _logger.Info("Added {ItemCount} items to Cart Id={CartId}",
                    cart.CartItems.Count, cartSaveResult.Entity.Id);

            var result = await context.Carts
                .Include(x => x.CartItems)
                .FirstAsync(x => x.Id == cartSaveResult.Entity.Id);
            _logger.Info("Successfully created Cart with Id={CartId}", result.Id);

            return result.ToDto();
        }
    }
}