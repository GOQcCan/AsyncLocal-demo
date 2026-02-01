namespace AsyncLocal_demo.Application.Orders
{
    /// <summary>
    /// Résultat de l'opération de traitement de commande.
    /// </summary>
    public sealed record OrderProcessingResult
    {
        public required Guid OrderId { get; init; }
        public required OrderProcessingStatus Status { get; init; }
        public string? Message { get; init; }
        public DateTimeOffset ProcessedAt { get; init; } = DateTimeOffset.UtcNow;

        public static OrderProcessingResult Success(Guid orderId, string? message = null) => new()
        {
            OrderId = orderId,
            Status = OrderProcessingStatus.Completed,
            Message = message
        };

        public static OrderProcessingResult Failed(Guid orderId, string message) => new()
        {
            OrderId = orderId,
            Status = OrderProcessingStatus.Failed,
            Message = message
        };

        public static OrderProcessingResult NotFound(Guid orderId) => new()
        {
            OrderId = orderId,
            Status = OrderProcessingStatus.NotFound,
            Message = "Commande introuvable"
        };

        public static OrderProcessingResult InProgress(Guid orderId) => new()
        {
            OrderId = orderId,
            Status = OrderProcessingStatus.Processing,
            Message = "Traitement en cours"
        };
    }
}