namespace PaymentSystem.Domain.Exceptions
{
    public class EntityNotFoundException(string message) : Exception(message);
}