using AsyncLocal_demo.Core.Context;
using System.Security.Claims;

namespace AsyncLocal_demo.Api;

/// <summary>
/// Middleware qui capture le contexte HTTP et le stocke dans l'ExecutionContext.
/// Permet aux Background Services de récupérer le contexte original.
/// </summary>
public sealed class ExecutionContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext httpContext, IExecutionContextAccessor contextAccessor)
    {
        // Capture le contexte de la requête HTTP
        var ctx = contextAccessor.Current;

        ctx.UserId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        ctx.TenantId = httpContext.User.FindFirst("tenant_id")?.Value
                     ?? httpContext.Request.Headers["X-Tenant-Id"].FirstOrDefault();
        ctx.CorrelationId = httpContext.TraceIdentifier;

        try
        {
            await next(httpContext);
        }
        finally
        {
            ctx.Clear(); // Nettoie le contexte après la requête
        }
    }
}