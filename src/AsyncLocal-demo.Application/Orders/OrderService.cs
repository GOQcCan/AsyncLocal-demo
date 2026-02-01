using AsyncLocal_demo.Core.BackgroundProcessing;
using AsyncLocal_demo.Core.Context;
using Microsoft.Extensions.Logging;

namespace AsyncLocal_demo.Application.Orders;

public sealed class OrderService(
    IExecutionContext context,
    IOrderRepository repository,
    IBackgroundTaskQueue<Guid> processingQueue,
    ILogger<OrderService> logger) : IOrderService
{
    public async Task<OrderDto> CreateAsync(CreateOrderCommand command, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        ValidateContext();

        var order = new Order
        {
            Id = Guid.CreateVersion7(),
            TenantId = context.TenantId!,
            CreatedBy = context.UserId ?? "anonyme",
            CorrelationId = context.CorrelationId!,
            Items = [..command.Items.Select(i => new OrderItem
            {
                Id = Guid.CreateVersion7(),
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            })],
            CreatedAt = DateTime.UtcNow
        };

        await repository.AddAsync(order, ct);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Commande {OrderId} créée – TenantId : {TenantId}, UserId : {UserId}, Total : {Total:C}",
                order.Id, order.TenantId, order.CreatedBy, order.TotalAmount);
        }

        return ToDto(order);
    }

    public async Task<OrderDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        ValidateContext();

        var order = await repository.GetByIdAsync(id, ct);

        return order is null ? null : ToDetailDto(order);
    }

    public async Task<IReadOnlyList<OrderDto>> GetAllAsync(int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        ValidateContext();

        var orders = await repository.GetAllAsync(page, pageSize, ct);

        return [..orders.Select(ToDto)];
    }

    public async Task EnqueueForProcessingAsync(Guid orderId, CancellationToken ct = default)
    {
        ValidateContext();

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Mise en file de la commande {OrderId} pour un traitement en arrière‑plan – TenantId : {TenantId}, CorrelationId : {CorrelationId}",
                orderId, context.TenantId, context.CorrelationId);
        }

        // Le contexte est automatiquement capturé par la file d’attente
        await processingQueue.EnqueueAsync(orderId, ct);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Commande {OrderId} mise en file – Taille de la file : {QueueSize}",
                orderId, processingQueue.PendingCount);
        }
    }

    private void ValidateContext()
    {
        if (string.IsNullOrWhiteSpace(context.TenantId))
            throw new UnauthorizedAccessException("TenantId est requis");

        if (string.IsNullOrWhiteSpace(context.CorrelationId))
            throw new InvalidOperationException("CorrelationId est requis");
    }

    private static OrderDto ToDto(Order o) => new(
        o.Id, o.TenantId, o.CreatedBy, o.CorrelationId,
        o.Items.Count, o.TotalAmount, o.CreatedAt);

    private static OrderDetailDto ToDetailDto(Order o) => new(
        o.Id, o.TenantId, o.CreatedBy, o.CorrelationId,
        [.. o.Items.Select(i => new OrderItemDto(
            i.Id, i.ProductId, i.ProductName,
            i.Quantity, i.UnitPrice, i.TotalPrice))],
        o.TotalAmount, o.CreatedAt);
}