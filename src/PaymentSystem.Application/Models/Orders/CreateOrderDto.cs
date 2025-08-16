using PaymentSystem.Application.Models.Carts;

namespace PaymentSystem.Application.Models.Orders
{
    public class CreateOrderDto
    {
        public string? Name { get; set; }

        public long OrderNumber { get; set; }

        public long CustomerId { get; set; }

        public CartDto Cart { get; set; }
    }
}