namespace AsyncLocal_demo.Application.Orders;

public sealed record OrderDto(
    Guid Id,
    string TenantId,
    string CreatedBy,
    string CorrelationId,
    int ItemCount,
    decimal TotalAmount,
    DateTime CreatedAt);