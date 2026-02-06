using AsyncLocal.ExecutionContext.Abstractions;
using Microsoft.AspNetCore.Http;

namespace AsyncLocal.ExecutionContext;

/// <summary>
/// Fournit le HttpContext réel depuis une requête HTTP.
/// Priorité maximale (0) - utilisé en premier si disponible.
/// </summary>
public sealed class DefaultHttpContextProvider(HttpContextAccessor innerAccessor) : IHttpContextProvider
{
    public int Priority => 0;

    public HttpContext? GetHttpContext() => innerAccessor.HttpContext;
}