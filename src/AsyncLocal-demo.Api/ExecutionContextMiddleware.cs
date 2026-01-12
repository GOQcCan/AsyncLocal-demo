using AsyncLocal_demo.Core.Context;

namespace AsyncLocal_demo.Api;

public sealed class ExecutionContextMiddleware(RequestDelegate next)
{
    public const string CorrelationIdHeader = "X-Correlation-Id";
    public const string TenantIdHeader = "X-Tenant-Id";
    public const string UserIdHeader = "X-User-Id";

    public async Task InvokeAsync(HttpContext http, IExecutionContext context)
    {
        try
        {
            context.CorrelationId = http.Request.Headers[CorrelationIdHeader].FirstOrDefault()
                ?? Guid.CreateVersion7().ToString("N")[..16].ToUpperInvariant();
            context.TenantId = http.Request.Headers[TenantIdHeader].FirstOrDefault();
            context.UserId = http.Request.Headers[UserIdHeader].FirstOrDefault();

            http.Response.OnStarting(() =>
            {
                http.Response.Headers[CorrelationIdHeader] = context.CorrelationId;
                return Task.CompletedTask;
            });

            await next(http);
        }
        finally
        {
            context.Clear();
        }
    }
}