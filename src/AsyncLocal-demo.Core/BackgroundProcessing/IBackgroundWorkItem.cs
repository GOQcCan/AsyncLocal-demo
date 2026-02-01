namespace AsyncLocal_demo.Core.BackgroundProcessing;

/// <summary>
/// Représente un élément de travail avec le contexte d'exécution capturé pour le traitement en arrière-plan.
/// </summary>
public interface IBackgroundWorkItem<out TPayload>
{
    /// <summary>
    /// La charge utile à traiter.
    /// </summary>
    TPayload Payload { get; }

    /// <summary>
    /// L'identifiant du tenant capturé au moment de la mise en file d'attente.
    /// </summary>
    string? TenantId { get; }

    /// <summary>
    /// L'identifiant de l'utilisateur capturé au moment de la mise en file d'attente.
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// L'identifiant de corrélation capturé au moment de la mise en file d'attente.
    /// </summary>
    string? CorrelationId { get; }

    /// <summary>
    /// L'horodatage de création de l'élément de travail.
    /// </summary>
    DateTimeOffset CreatedAt { get; }
}