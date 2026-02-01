namespace AsyncLocal_demo.Application.Orders;

/// <summary>
/// Service pour le traitement asynchrone des commandes en arrière-plan.
/// Suit le principe de responsabilité unique - séparé de la création de commandes.
/// </summary>
public interface IOrderProcessor
{
    /// <summary>
    /// Traite une commande (ex: validation, paiement, vérification des stocks).
    /// </summary>
    Task<OrderProcessingResult> ProcessAsync(Guid orderId, CancellationToken ct = default);
}