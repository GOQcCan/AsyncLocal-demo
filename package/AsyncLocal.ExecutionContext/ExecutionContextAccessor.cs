using AsyncLocal.ExecutionContext.Abstractions;

namespace AsyncLocal.ExecutionContext;

/// <summary>
/// Fournit l'accès au contexte d'exécution courant.
/// </summary>
public sealed class ExecutionContextAccessor(AsyncLocalExecutionContext context) : IExecutionContextAccessor
{
    public IExecutionContext Current => context;
}