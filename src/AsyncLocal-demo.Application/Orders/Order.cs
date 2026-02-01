namespace AsyncLocal_demo.Application.Orders;

public sealed class Order
{
    public Guid Id { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public List<OrderItem> Items { get; set; } = [];
    public DateTime CreatedAt { get; set; }

    // Champs de traitement
    public OrderProcessingStatus Status { get; set; } =  OrderProcessingStatus.Pending;
    public DateTime? ProcessedAt { get; set; }
    public string? ProcessedBy { get; set; }

    public decimal TotalAmount => Items.Sum(i => i.TotalPrice);
}