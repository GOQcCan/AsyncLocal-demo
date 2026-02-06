namespace AsyncLocal.ExecutionContext.Abstractions;

public interface IExecutionContextAccessor
{
    IExecutionContext Current { get; }
}