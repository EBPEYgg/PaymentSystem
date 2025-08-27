namespace PaymentSystem.Domain.Models;

public enum OrderStatus
{
    Created = 1,
    Pending,
    Success,
    Reject,
    Fail,
    Error
}