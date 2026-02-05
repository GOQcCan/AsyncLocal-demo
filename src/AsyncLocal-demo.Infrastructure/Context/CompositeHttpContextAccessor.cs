using AsyncLocal_demo.Core.Context;
using Microsoft.AspNetCore.Http;

namespace AsyncLocal_demo.Infrastructure.Context;

/// <summary>
/// Composite qui délègue aux providers selon leur priorité.
/// Ouvert à l'extension, fermé à la modification.
/// </summary>
public sealed class CompositeHttpContextAccessor(IEnumerable<IHttpContextProvider> providers) : IHttpContextAccessor
{
    private static readonly AsyncLocal<HttpContext?> _explicitContext = new();
    private readonly IEnumerable<IHttpContextProvider> _providers = providers.OrderBy(p => p.Priority);

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