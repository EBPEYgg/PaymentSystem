using PaymentSystem.Application.Models.Carts;
using PaymentSystem.Application.Models.Orders;
using PaymentSystem.Domain.Entities;
using PaymentSystem.Domain.Models;

namespace PaymentSystem.Application.Mappers
{
    public static class OrdersMapper
    {
        public static OrderDto ToDto(this OrderEntity entity, CartEntity? cart = null)
        {
            return new OrderDto
            {
                Id = entity.Id,
                CustomerId = entity.CustomerId!.Value,
                Cart = cart == null ? entity.Cart?.ToDto() : cart.ToDto(),
                Name = entity.Name,
                OrderNumber = entity.OrderNumber,
                OrderStatus = entity.OrderStatus.ToString()
            };
        }

        public static OrderEntity ToEntity(this CreateOrderDto entity, CartDto? cart = null)
        {
            return new OrderEntity
            {
                CustomerId = entity.CustomerId,
                Cart = cart?.ToEntity(),
                Name = entity.Name,
                OrderNumber = entity.OrderNumber,
                OrderStatus = (OrderStatus)Enum.Parse(typeof(OrderStatus), entity.OrderStatus)
            };
        }
    }
}