using AsyncLocal_demo.Core.BackgroundProcessing;
using AsyncLocal_demo.Core.Context;

namespace AsyncLocal_demo.Infrastructure.BackgroundProcessing;

/// <summary>
/// Record immuable représentant un élément de travail avec le contexte d'exécution capturé.
/// Utilise les records pour l'égalité par valeur et l'immuabilité.
/// </summary>
public sealed record BackgroundWorkItem<TPayload> : IBackgroundWorkItem<TPayload>
{
    public required TPayload Payload { get; init; }
    public string? TenantId { get; init; }
    public string? UserId { get; init; }
    public string? CorrelationId { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Méthode de fabrique pour créer un élément de travail avec le contexte capturé depuis le contexte d'exécution actuel.
    /// </summary>
    public static BackgroundWorkItem<TPayload> FromContext(TPayload payload, IExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentNullException.ThrowIfNull(context);

        return new BackgroundWorkItem<TPayload>
        {
            Payload = payload,
            TenantId = context.TenantId,
            UserId = context.UserId,
            CorrelationId = context.CorrelationId,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}