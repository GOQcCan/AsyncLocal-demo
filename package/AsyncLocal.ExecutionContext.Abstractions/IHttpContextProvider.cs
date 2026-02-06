using Microsoft.AspNetCore.Http;

namespace AsyncLocal.ExecutionContext.Abstractions;

public interface IHttpContextProvider
{
    int Priority { get; }
    HttpContext? GetHttpContext();
}