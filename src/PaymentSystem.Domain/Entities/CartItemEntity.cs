namespace PaymentSystem.Domain.Entities
{
    public class CartItemEntity : BaseEntity
    {
        public required string Name { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public CartEntity? Cart { get; set; }

        public long? CartId { get; set; }
    }
}