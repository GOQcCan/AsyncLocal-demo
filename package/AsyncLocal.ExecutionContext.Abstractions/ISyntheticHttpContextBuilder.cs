using Microsoft.AspNetCore.Http;

namespace AsyncLocal.ExecutionContext.Abstractions;

public interface ISyntheticHttpContextBuilder
{
    bool CanBuild(IExecutionContext context);
    HttpContext Build(IExecutionContext context);
}