using AsyncLocal_demo.Core.Context;

namespace AsyncLocal_demo.Infrastructure.Context;

public sealed class ExecutionContextAccessor : IExecutionContextAccessor
{
    private static readonly AsyncLocalExecutionContext Instance = new();

    public IExecutionContext Current => Instance;
}