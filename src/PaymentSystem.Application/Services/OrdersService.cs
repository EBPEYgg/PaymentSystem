using Microsoft.EntityFrameworkCore;
using NLog;
using PaymentSystem.Application.Abstractions;
using PaymentSystem.Application.Mappers;
using PaymentSystem.Application.Models.Orders;
using PaymentSystem.Domain.Data;
using PaymentSystem.Domain.Entities;

namespace PaymentSystem.Application.Services
{
    /// <summary>
    /// Сервис для работы с заказами.
    /// </summary>
    public class OrdersService(OrdersDbContext context, ICartsService cartsService) : IOrdersService
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public async Task<OrderDto> Create(CreateOrderDto order)
        {
            var orderByOrderNumber = await context.Orders
                .FirstOrDefaultAsync(x => x.OrderNumber == order.OrderNumber);

            if (order.Cart == null)
            {
                throw new ArgumentNullException();
            }

            var cart = await cartsService.Create(order.Cart);
            var entity = new OrderEntity
            {
                OrderNumber = order.OrderNumber,
                Name = order.Name,
                CustomerId = order.CustomerId,
                CartId = cart.Id
            };

            var orderSaveResult = await context.Orders.AddAsync(entity);
            await context.SaveChangesAsync();

            var orderEntityResult = orderSaveResult.Entity;
            return orderEntityResult.ToDto();
        }

        public async Task<List<OrderDto>> GetAll()
        {
            _logger.Info("Retrieving all orders from the database.");
            var entity = await context.Orders
                .Include(o => o.Cart)
                .ThenInclude(c => c.CartItems)
                .ToListAsync();

            return entity.Select(x => x.ToDto()).ToList();
        }

        public async Task<OrderDto> GetById(long orderId)
        {
            var entity = await context.Orders
                .Include(o => o.Cart)
                .ThenInclude(c => c.CartItems)
                .FirstOrDefaultAsync(x => x.Id == orderId);

            return entity.ToDto();
        }

        public async Task<List<OrderDto>> GetByUser(long customerId)
        {
            var entity = await context.Orders
                .Include(o => o.Cart)
                .ThenInclude(c => c.CartItems)
                .Where(x => x.CustomerId == customerId)
                .ToListAsync();

            return entity.Select(x => x.ToDto()).ToList();
        }

        public async Task Reject(long orderId)
        {
            throw new NotImplementedException();
        }
    }
}