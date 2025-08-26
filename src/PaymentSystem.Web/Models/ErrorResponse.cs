using System.Text.Json;

namespace PaymentSystem.Web.Models;

public class ErrorResponse
{
    public required int StatusCode { get; init; }

    public required string Message { get; init; }

    public string Description { get; init; } = string.Empty;

    public string TraceId { get; init; } = string.Empty;

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}