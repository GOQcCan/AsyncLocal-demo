using Microsoft.Extensions.Logging;

namespace AsyncLocal_demo.Application.Orders;

/// <summary>
/// Implémentation du processeur de commandes.
/// </summary>
public sealed class OrderProcessor(
    IOrderRepository repository,
    ILogger<OrderProcessor> logger) : IOrderProcessor
{
    public async Task<OrderProcessingResult> ProcessAsync(Guid orderId, string? userId, CancellationToken ct = default)
    {
        var order = await repository.GetByIdAsync(orderId, ct);

        if (order is null)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning("Commande {OrderId} introuvable pour le traitement", orderId);
            }
            return OrderProcessingResult.NotFound(orderId);
        }

        if (order.Status == OrderProcessingStatus.Processing)
            return OrderProcessingResult.InProgress(orderId);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Traitement de la commande {OrderId} – Contexte : TenantId={TenantId}, CorrelationId={CorrelationId}",
                orderId,
                order.TenantId,
                order.CorrelationId);
        }

        // Simuler le traitement (paiement, inventaire, etc.)
        await Task.Delay(20000, ct);

        order.Status = OrderProcessingStatus.Completed;
        order.ProcessedAt = DateTime.UtcNow;
        order.ProcessedBy = userId;

        await repository.UpdateAsync(order, ct);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Commande {OrderId} traitée avec succès pour le TenantId {TenantId}",
                orderId, order.TenantId);
        }

        return OrderProcessingResult.Success(orderId, "Commande traitée avec succès");
    }
}