using AsyncLocal.ExecutionContext.Abstractions;
using AsyncLocal_demo.Application.Orders;
using AsyncLocal_demo.Core.BackgroundProcessing;
using AsyncLocal_demo.Core.Context;
using Microsoft.AspNetCore.Mvc;

namespace AsyncLocal_demo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class OrdersController(
    IOrderService orderService,
    IBackgroundTaskQueue<Guid> processingQueue,
    IExecutionContext context) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateOrderCommand command, CancellationToken ct)
    {
        var result = await orderService.CreateAsync(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await orderService.GetByIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<OrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await orderService.GetAllAsync(page, pageSize, ct);
        return Ok(result);
    }

    /// <summary>
    /// Met en file d'attente une commande pour traitement en arrière-plan.
    /// Le contexte d'exécution actuel (TenantId, UserId, CorrelationId) est capturé
    /// et sera restauré lorsque le service d'arrière-plan traitera la commande.
    /// </summary>
    [HttpPost("{id:guid}/process")]
    [ProducesResponseType(typeof(EnqueuedResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> EnqueueForProcessing(Guid id, CancellationToken ct)
    {
        await orderService.EnqueueForProcessingAsync(id, ct);

        return Accepted(new EnqueuedResponse(
            id,
            context.GetTenantId()!,
            context.CorrelationId!,
            processingQueue.PendingCount,
            "Commande mise en file d'attente pour traitement en arrière-plan. Le contexte sera préservé."));
    }

    [HttpGet("context")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult GetContext() => Ok(new
    {
        context.CorrelationId,
        TenantId = context.GetTenantId(),
        UserId = context.GetUserId(),
        Timestamp = DateTime.UtcNow
    });

    [HttpGet("queue/status")]
    [ProducesResponseType(typeof(QueueStatusResponse), StatusCodes.Status200OK)]
    public IActionResult GetQueueStatus() => Ok(new QueueStatusResponse(
        processingQueue.PendingCount,
        DateTime.UtcNow));
}

public sealed record EnqueuedResponse(
    Guid OrderId,
    string TenantId,
    string CorrelationId,
    int QueuePosition,
    string Message);

public sealed record QueueStatusResponse(
    int PendingCount,
    DateTime Timestamp);