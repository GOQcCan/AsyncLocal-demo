using AsyncLocal.ExecutionContext.Abstractions;
using AsyncLocal.ExecutionContext.Internal;

namespace AsyncLocal.ExecutionContext;

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