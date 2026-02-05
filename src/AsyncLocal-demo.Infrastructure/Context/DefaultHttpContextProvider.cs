using AsyncLocal_demo.Core.Context;
using Microsoft.AspNetCore.Http;

namespace AsyncLocal_demo.Infrastructure.Context;

/// <summary>
/// Fournit le HttpContext réel depuis une requête HTTP.
/// </summary>
public sealed class DefaultHttpContextProvider(HttpContextAccessor innerAccessor) : IHttpContextProvider
{
    public int Priority => 0; // Priorité maximale

    public HttpContext? GetHttpContext() => innerAccessor.HttpContext;
}