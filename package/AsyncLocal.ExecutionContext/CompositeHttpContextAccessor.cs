using AsyncLocal.ExecutionContext.Abstractions;
using Microsoft.AspNetCore.Http;

namespace AsyncLocal.ExecutionContext;

/// <summary>
/// Composite qui délègue aux providers selon leur priorité.
/// Ouvert à l'extension, fermé à la modification (Open/Closed Principle).
/// </summary>
public sealed class CompositeHttpContextAccessor(IEnumerable<IHttpContextProvider> providers) : IHttpContextAccessor
{
    private static readonly AsyncLocal<HttpContext?> _explicitContext = new();
    private readonly IHttpContextProvider[] _providers = [.. providers.OrderBy(p => p.Priority)];

    public HttpContext? HttpContext
    {
        get
        {
            // Priorité au contexte explicitement défini
            if (_explicitContext.Value is not null)
                return _explicitContext.Value;

            // Parcourir les providers par priorité
            foreach (var provider in _providers)
            {
                var context = provider.GetHttpContext();
                if (context is not null)
                    return context;
            }

            return null;
        }
        set => _explicitContext.Value = value;
    }
}