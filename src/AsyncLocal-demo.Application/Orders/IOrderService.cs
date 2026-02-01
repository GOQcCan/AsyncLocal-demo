namespace AsyncLocal_demo.Application.Orders;

public interface IOrderService
{
    Task<OrderDto> CreateAsync(CreateOrderCommand command, CancellationToken ct = default);
    Task<OrderDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<OrderDto>> GetAllAsync(int page = 1, int pageSize = 20, CancellationToken ct = default);

    /// <summary>
    /// Met en file d'attente une commande pour traitement en arrière-plan.
    /// Le contexte d'exécution actuel (TenantId, UserId, CorrelationId) est capturé.
    /// </summary>
    Task EnqueueForProcessingAsync(Guid orderId, CancellationToken ct = default);
}