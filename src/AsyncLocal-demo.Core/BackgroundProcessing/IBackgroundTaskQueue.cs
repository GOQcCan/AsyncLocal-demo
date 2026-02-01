namespace AsyncLocal_demo.Core.BackgroundProcessing;

/// <summary>
/// Abstraction pour une file d'attente de tâches en arrière-plan qui préserve le contexte d'exécution.
/// Suit le principe de ségrégation des interfaces (ISP) - sépare les responsabilités de lecture et d'écriture.
/// </summary>
public interface IBackgroundTaskQueue<TPayload>
{
    /// <summary>
    /// Met en file d'attente un élément de travail pour traitement en arrière-plan.
    /// Le contexte d'exécution actuel est automatiquement capturé.
    /// </summary>
    ValueTask EnqueueAsync(TPayload payload, CancellationToken ct = default);

    /// <summary>
    /// Retire le prochain élément de travail de la file pour traitement.
    /// </summary>
    ValueTask<IBackgroundWorkItem<TPayload>> DequeueAsync(CancellationToken ct);

    /// <summary>
    /// Retourne un énumérable asynchrone pour consommer tous les éléments de travail.
    /// </summary>
    IAsyncEnumerable<IBackgroundWorkItem<TPayload>> ReadAllAsync(CancellationToken ct);

    /// <summary>
    /// Obtient le nombre actuel d'éléments en attente.
    /// </summary>
    int PendingCount { get; }
}