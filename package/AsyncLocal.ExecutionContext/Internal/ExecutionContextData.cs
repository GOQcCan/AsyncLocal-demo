using System.Collections.Frozen;

namespace AsyncLocal.ExecutionContext.Internal;

internal sealed record ExecutionContextData
{
    public static ExecutionContextData Empty { get; } = new();

    public string? CorrelationId { get; init; }
    public FrozenDictionary<string, object> CustomData { get; init; } =
        FrozenDictionary<string, object>.Empty;

    public ExecutionContextData With(string key, object value)
    {
        var dict = CustomData.ToDictionary(x => x.Key, x => x.Value);
        dict[key] = value;
        return this with { CustomData = dict.ToFrozenDictionary() };
    }
}