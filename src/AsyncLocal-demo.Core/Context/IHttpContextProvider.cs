using Microsoft.AspNetCore.Http;

namespace AsyncLocal_demo.Core.Context;

/// <summary>
/// Fournit un HttpContext selon une stratégie spécifique.
/// </summary>
public interface IHttpContextProvider
{
    /// <summary>
    /// Ordre de priorité (plus petit = priorité plus haute).
    /// </summary>
    int Priority { get; }
    
    /// <summary>
    /// Tente de fournir un HttpContext.
    /// </summary>
    HttpContext? GetHttpContext();
}