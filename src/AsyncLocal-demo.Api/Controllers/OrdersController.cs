using AsyncLocal_demo.Application.Orders;
using AsyncLocal_demo.Core.Context;
using Microsoft.AspNetCore.Mvc;

namespace AsyncLocal_demo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class OrdersController(
    IOrderService orderService,
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

    [HttpGet("context")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult GetContext() => Ok(new
    {
        context.CorrelationId,
        context.TenantId,
        context.UserId,
        Timestamp = DateTime.UtcNow
    });
}