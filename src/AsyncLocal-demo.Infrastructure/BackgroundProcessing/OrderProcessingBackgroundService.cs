using AsyncLocal_demo.Application.Orders;
using AsyncLocal_demo.Core.BackgroundProcessing;
using AsyncLocal_demo.Core.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AsyncLocal_demo.Infrastructure.BackgroundProcessing;

/// <summary>
/// Service d'arrière-plan qui traite les commandes depuis la file d'attente.
/// Démontre la restauration correcte du contexte AsyncLocal pour le traitement en arrière-plan.
/// 
/// Patron clé:
/// 1. Les éléments de travail sont retirés avec leur contexte capturé
/// 2. Un nouveau scope DI est créé pour chaque élément de travail
/// 3. Le contexte AsyncLocal est restauré avant le traitement
/// 4. Le processeur s'exécute avec le contexte de la requête originale
/// </summary>
public sealed class OrderProcessingBackgroundService(
    IBackgroundTaskQueue<Guid> queue,
    IServiceScopeFactory scopeFactory,
    ILogger<OrderProcessingBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Service de traitement des commandes en arrière-plan démarré");

        await foreach (var workItem in queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                await ProcessWorkItemAsync(workItem, stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                if (logger.IsEnabled(LogLevel.Error))
                {
                    logger.LogError(ex,
                        "Erreur lors du traitement de la commande {OrderId} pour le TenantId {TenantId}",
                        workItem.Payload,
                        workItem.TenantId);
                }
            }
        }

        logger.LogInformation("Service de traitement des commandes en arrière-plan arrêté");
    }

    private async Task ProcessWorkItemAsync(
        IBackgroundWorkItem<Guid> workItem,
        CancellationToken ct)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "Commande {OrderId} retirée pour traitement - TenantId: {TenantId}, UserId: {UserId}, CorrelationId: {CorrelationId}, MiseEnFile: {QueuedAt}",
                workItem.Payload,
                workItem.TenantId,
                workItem.UserId,
                workItem.CorrelationId,
                workItem.CreatedAt);
        }

        // Créer un nouveau scope pour cet élément de travail
        await using var scope = scopeFactory.CreateAsyncScope();

        // Restaurer le contexte capturé dans AsyncLocal
        var context = scope.ServiceProvider.GetRequiredService<IExecutionContext>();
        RestoreContext(context, workItem);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Contexte restauré pour la commande {OrderId} - TenantId: {TenantId}",
                workItem.Payload,
                context.TenantId);
        }

        // Traiter avec le contexte restauré
        var processor = scope.ServiceProvider.GetRequiredService<IOrderProcessor>();
        var result = await processor.ProcessAsync(workItem.Payload, ct);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Traitement de la commande {OrderId} terminé avec le statut {Status}: {Message}",
                result.OrderId,
                result.Status,
                result.Message);
        }
    }

    private static void RestoreContext(IExecutionContext context, IBackgroundWorkItem<Guid> workItem)
    {
        context.TenantId = workItem.TenantId;
        context.UserId = workItem.UserId;
        context.CorrelationId = workItem.CorrelationId;
    }
}