using System.Security.Claims;
using AsyncLocal.ExecutionContext.Abstractions;
using AsyncLocal_demo.Core.Context;
using Microsoft.AspNetCore.Http;

namespace AsyncLocal_demo.Infrastructure.Context;

/// <summary>
/// Construit un HttpContext synthétique avec les claims spécifiques à cette solution.
/// </summary>
public sealed class DemoSyntheticHttpContextBuilder : ISyntheticHttpContextBuilder
{
    public bool CanBuild(IExecutionContext context)
        => !string.IsNullOrEmpty(context.GetUserId()) || !string.IsNullOrEmpty(context.GetTenantId());

    public HttpContext Build(IExecutionContext context)
    {
        var httpContext = new DefaultHttpContext();
        var claims = new List<Claim>();

        if (context.GetUserId() is { } userId)
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));

        if (context.GetTenantId() is { } tenantId)
            claims.Add(new Claim("tenant_id", tenantId));

        if (!string.IsNullOrEmpty(context.CorrelationId))
            httpContext.TraceIdentifier = context.CorrelationId;

        if (claims.Count > 0)
        {
            var identity = new ClaimsIdentity(claims, "BackgroundService");
            httpContext.User = new ClaimsPrincipal(identity);
        }

        return httpContext;
    }
}