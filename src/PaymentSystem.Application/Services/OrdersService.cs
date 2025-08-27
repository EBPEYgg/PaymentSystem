using Microsoft.EntityFrameworkCore;
using NLog;
using PaymentSystem.Application.Abstractions;
using PaymentSystem.Application.Mappers;
using PaymentSystem.Application.Models.Orders;
using PaymentSystem.Domain.Data;
using PaymentSystem.Domain.Entities;
using PaymentSystem.Domain.Exceptions;
using PaymentSystem.Domain.Models;

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
            _logger.Debug("Received CreateOrderDto: {@Order}.", order);

            var existingOrder = await context.Orders.FirstOrDefaultAsync(
                x => x.OrderNumber == order.OrderNumber && x.MerchantId == order.MerchantId);

            if (existingOrder != null)
            {
                _logger.Warn("Order with OrderNumber={OrderNumber} is exist for MerchantId={MerchantId}.", 
                    order.OrderNumber, order.MerchantId);
                throw new DuplicateEntityException($"Order with orderNumber={order.OrderNumber} is exist " +
                    $"for merchant id={order.MerchantId}.");
            }

            if (order.Cart == null)
            {
                _logger.Error("Attempt to create order without a cart. OrderNumber={OrderNumber}.", order.OrderNumber);
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
            _logger.Info("Successfully created a new order with id={OrderId}, OrderNumber={OrderNumber}.", 
                entity.Id, entity.OrderNumber);
            return orderEntityResult.ToDto();
        }

        public async Task<List<OrderDto>> GetAll()
        {
            _logger.Info("Retrieving all orders from the database.");
            var entities = await context.Orders
                .Include(o => o.Cart)
                .ThenInclude(c => c.CartItems)
                .ToListAsync();

            _logger.Info("Successfully retrieved {Count} orders from the database.", entities.Count());
            return entities.Select(x => x.ToDto()).ToList();
        }

        public async Task<OrderDto> GetById(long orderId)
        {
            _logger.Info("Retrieving order with id={OrderId}.", orderId);
            var entity = await context.Orders
                .Include(o => o.Cart)
                .ThenInclude(c => c.CartItems)
                .FirstOrDefaultAsync(x => x.Id == orderId);

            if (entity == null)
            {
                throw new EntityNotFoundException($"Order entity with id={orderId} not found.");
            }

            _logger.Info("Successfully retrieved order with id={OrderId}.", orderId);
            return entity.ToDto();
        }

        public async Task<List<OrderDto>> GetByUser(long customerId)
        {
            _logger.Info("Retrieving orders by customer id={CustomerId}.", customerId);
            var entities = await context.Orders
                .Include(o => o.Cart)
                .ThenInclude(c => c.CartItems)
                .Where(x => x.CustomerId == customerId)
                .ToListAsync();

            _logger.Info("Successfully retrieved {Count} orders for customer id={CustomerId}.", entities.Count, customerId);
            return entities.Select(x => x.ToDto()).ToList();
        }

        public async Task Reject(long orderId)
        {
            _logger.Info("Rejecting order with id={OrderId}.", orderId);
            var order = await context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                throw new EntityNotFoundException($"Order with id={orderId} not found.");
            }

            order.OrderStatus = OrderStatus.Reject;
            await context.SaveChangesAsync();
            _logger.Info("Successfully rejecting order with id={OrderId}.", orderId);
        }
    }
}