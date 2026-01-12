namespace AsyncLocal_demo.Core.Context
{
    public interface IExecutionContextAccessor
    {
        IExecutionContext Current { get; }
    }
}