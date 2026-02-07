using Microsoft.AspNetCore.Builder;

namespace AsyncLocal.ExecutionContext;

public static class ExecutionContextMiddlewareExtensions
{
    public static IApplicationBuilder UseExecutionContext<TMiddleware>(this IApplicationBuilder app) =>
        app.UseMiddleware<TMiddleware>();
}