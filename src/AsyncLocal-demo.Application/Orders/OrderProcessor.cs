using AsyncLocal_demo.Core.Context;
using Microsoft.Extensions.Logging;

namespace AsyncLocal_demo.Application.Orders;

/// <summary>
/// Implémentation du processeur de commandes.
/// Démontre que le contexte AsyncLocal est correctement restauré lors du traitement en arrière-plan.
/// </summary>
public sealed class OrderProcessor(
    IExecutionContext context,
    IOrderRepository repository,
    ILogger<OrderProcessor> logger) : IOrderProcessor
{
    public async Task<OrderProcessingResult> ProcessAsync(Guid orderId, CancellationToken ct = default)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Traitement de la commande {OrderId} – Contexte : TenantId={TenantId}, UserId={UserId}, CorrelationId={CorrelationId}",
                orderId,
                context.TenantId,
                context.UserId,
                context.CorrelationId);
        }

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

        // Vérifier que l'isolation du tenant est maintenue
        if (order.TenantId != context.TenantId)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning(
                    "Incohérence de TenantId pour la commande {OrderId} : Commande TenantId={OrderTenant}, Contexte TenantId={ContextTenant}",
                    orderId, order.TenantId, context.TenantId);
            }

            return OrderProcessingResult.Failed(orderId, "Incohérence de TenantId - accès refusé");
        }

        // Simuler le traitement (paiement, inventaire, etc.)
        await Task.Delay(5000, ct);

        order.Status = OrderProcessingStatus.Completed;
        order.ProcessedAt = DateTime.UtcNow;
        order.ProcessedBy = context.UserId;

        await repository.UpdateAsync(order, ct);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Commande {OrderId} traitée avec succès pour le TenantId {TenantId}",
                orderId, context.TenantId);
        }

        return OrderProcessingResult.Success(orderId, "Commande traitée avec succès");
    }
}