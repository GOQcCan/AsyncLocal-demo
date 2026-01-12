using AsyncLocal_demo.Application.Orders;
using AsyncLocal_demo.Core.Context;
using Microsoft.EntityFrameworkCore;

namespace AsyncLocal_demo.Infrastructure.Persistence;

public sealed class OrderRepository(
    AppDbContext db,
    IExecutionContext context) : IOrderRepository
{
    private IQueryable<Order> TenantOrders => db.Orders
        .Where(o => o.TenantId == context.TenantId);

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await TenantOrders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<IReadOnlyList<Order>> GetAllAsync(int page = 1, int pageSize = 20, CancellationToken ct = default) =>
        await TenantOrders
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public async Task AddAsync(Order order, CancellationToken ct = default)
    {
        order.TenantId = context.TenantId ?? throw new InvalidOperationException("TenantId required");
        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);
    }
}