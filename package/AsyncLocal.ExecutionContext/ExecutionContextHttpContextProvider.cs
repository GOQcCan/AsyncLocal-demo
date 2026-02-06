using AsyncLocal.ExecutionContext.Abstractions;
using Microsoft.AspNetCore.Http;

namespace AsyncLocal.ExecutionContext;

public sealed class ExecutionContextHttpContextProvider(
    IExecutionContextAccessor contextAccessor,
    ISyntheticHttpContextBuilder httpContextBuilder) : IHttpContextProvider
{
    public int Priority => 10;

    public HttpContext? GetHttpContext()
    {
        var ctx = contextAccessor.Current;
        return httpContextBuilder.CanBuild(ctx) ? httpContextBuilder.Build(ctx) : null;
    }
}