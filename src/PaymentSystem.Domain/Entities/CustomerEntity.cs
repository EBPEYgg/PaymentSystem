namespace PaymentSystem.Domain.Entities
{
    public class CustomerEntity : BaseEntity
    {
        public string? Email { get; set; }

        public string? Phone { get; set; }

        public required string FirstName { get; set; }

        public required string LastName { get; set; }

        public string? MiddleName { get; set; }

        public DateTime? BirthDate { get; set; }

        public List<OrderEntity>? Orders { get; set; }
    }
}