using AsyncLocal_demo.Core.Context;

namespace AsyncLocal_demo.Infrastructure.Context;

public sealed class AsyncLocalExecutionContext : IExecutionContext
{
    private static readonly AsyncLocal<ExecutionContextData> Data = new();

    private static ExecutionContextData Current
    {
        get => Data.Value ?? ExecutionContextData.Empty;
        set => Data.Value = value;
    }

    public string? CorrelationId
    {
        get => Current.CorrelationId;
        set => Current = Current with { CorrelationId = value };
    }

    public string? UserId
    {
        get => Current.UserId;
        set => Current = Current with { UserId = value };
    }

    public string? TenantId
    {
        get => Current.TenantId;
        set => Current = Current with { TenantId = value };
    }

    public T? Get<T>(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return Current.CustomData.TryGetValue(key, out var val) && val is T t ? t : default;
    }

    public void Set<T>(string key, T value) where T : notnull
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);
        Current = Current.With(key, value);
    }

    public void Clear() => Data.Value = ExecutionContextData.Empty;
}