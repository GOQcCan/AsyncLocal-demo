namespace AsyncLocal_demo.Core.Context;

public interface IExecutionContext
{
    string? CorrelationId { get; set; }
    string? UserId { get; set; }
    string? TenantId { get; set; }
    T? Get<T>(string key);
    void Set<T>(string key, T value) where T : notnull;
    void Clear();
}