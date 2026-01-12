using System.ComponentModel.DataAnnotations;

namespace AsyncLocal_demo.Application.Orders;

public sealed record CreateOrderCommand
{
    [Required, MinLength(1)]
    public required IReadOnlyList<OrderItemCommand> Items { get; init; }
}