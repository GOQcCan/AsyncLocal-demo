namespace AsyncLocal.ExecutionContext.Abstractions;

public interface IExecutionContext
{
    string? CorrelationId { get; set; }
    T? Get<T>(string key);
    void Set<T>(string key, T value) where T : notnull;
    void Clear();
}