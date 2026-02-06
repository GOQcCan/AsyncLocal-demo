using AsyncLocal.ExecutionContext.Abstractions;
using AsyncLocal_demo.Core.Context;
using System.Security.Claims;

namespace AsyncLocal_demo.Api;

/// <summary>
/// Middleware qui capture le contexte HTTP et le stocke dans l'ExecutionContext.
/// </summary>
public sealed class ExecutionContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext httpContext, IExecutionContextAccessor contextAccessor)
    {
        var ctx = contextAccessor.Current;

        ctx.SetUserId(httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        ctx.SetTenantId(httpContext.User.FindFirst("tenant_id")?.Value
                     ?? httpContext.Request.Headers["X-Tenant-Id"].FirstOrDefault());
        ctx.CorrelationId = httpContext.TraceIdentifier;

        try
        {
            await next(httpContext);
        }
        finally
        {
            ctx.Clear();
        }
    }
}