using System.ComponentModel.DataAnnotations;

namespace AsyncLocal_demo.Application.Orders;

public sealed record OrderItemCommand(
    [Required] string ProductId,
    [Required] string ProductName,
    [Range(1, 10000)] int Quantity,
    [Range(0.01, 1000000)] decimal UnitPrice);