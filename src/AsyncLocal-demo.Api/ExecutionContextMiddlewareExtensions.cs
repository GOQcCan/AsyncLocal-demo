namespace AsyncLocal_demo.Api;

public static class ExecutionContextMiddlewareExtensions
{
    public static IApplicationBuilder UseExecutionContext(this IApplicationBuilder app) =>
        app.UseMiddleware<ExecutionContextMiddleware>();
}