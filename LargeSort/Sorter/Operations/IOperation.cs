namespace Sorter.Operations;

public interface IOperation
{
    Task ExecuteAsync();
    string OperationName { get; }
}
