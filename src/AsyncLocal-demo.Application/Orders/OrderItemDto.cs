namespace AsyncLocal_demo.Application.Orders;

public sealed record OrderItemDto(
    Guid Id,
    string ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice);