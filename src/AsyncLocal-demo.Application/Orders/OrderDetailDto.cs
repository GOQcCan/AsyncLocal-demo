namespace AsyncLocal_demo.Application.Orders;

public sealed record OrderDetailDto(
    Guid Id,
    string TenantId,
    string CreatedBy,
    string CorrelationId,
    IReadOnlyList<OrderItemDto> Items,
    decimal TotalAmount,
    DateTime CreatedAt,
    OrderProcessingStatus Status,
    DateTime? ProcessedAt,
    string? ProcessedBy);