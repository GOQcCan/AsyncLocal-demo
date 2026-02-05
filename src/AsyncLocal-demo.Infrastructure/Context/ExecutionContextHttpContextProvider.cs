using AsyncLocal_demo.Core.Context;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace AsyncLocal_demo.Infrastructure.Context;

/// <summary>
/// Fournit un HttpContext synthétique depuis l'ExecutionContext (AsyncLocal).
/// </summary>
public sealed class ExecutionContextHttpContextProvider(IExecutionContextAccessor executionContextAccessor) : IHttpContextProvider
{
    public int Priority => 10; // Fallback après le HttpContext réel

    public HttpContext? GetHttpContext()
    {
        var ctx = executionContextAccessor.Current;
        
        if (string.IsNullOrEmpty(ctx.UserId) && string.IsNullOrEmpty(ctx.TenantId))
            return null;

        return CreateSyntheticHttpContext(ctx);
    }

    private static HttpContext CreateSyntheticHttpContext(IExecutionContext ctx)
    {
        var httpContext = new DefaultHttpContext();
        var claims = new List<Claim>();

        if (!string.IsNullOrEmpty(ctx.UserId))
            claims.Add(new Claim(ClaimTypes.NameIdentifier, ctx.UserId));

        if (!string.IsNullOrEmpty(ctx.TenantId))
            claims.Add(new Claim("tenant_id", ctx.TenantId));

        if (!string.IsNullOrEmpty(ctx.CorrelationId))
            httpContext.TraceIdentifier = ctx.CorrelationId;

        if (claims.Count > 0)
        {
            var identity = new ClaimsIdentity(claims, "BackgroundService");
            httpContext.User = new ClaimsPrincipal(identity);
        }

        return httpContext;
    }
}