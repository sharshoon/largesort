namespace Sorter;

public class MergeSorter<T>
{
    private readonly PriorityQueue<IAsyncEnumerator<T>, T> _priorityQueue;
    private readonly IComparer<T> _comparer;
    
    public MergeSorter(
        IReadOnlyCollection<IAsyncEnumerator<T>> sources,
        IComparer<T> comparer
    )
    {
        if (sources == null || sources.Count == 0)
            throw new ArgumentException("At least one source is required for merge sorting", nameof(sources));
            
        _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
        _priorityQueue = new PriorityQueue<IAsyncEnumerator<T>, T>(comparer);
        
        foreach(var source in sources){
            if(source.Current != null)
                _priorityQueue.Enqueue(source, source.Current);
        }
    }

    public async IAsyncEnumerable<T> MergeSort(){
        while(_priorityQueue.Count > 0){
            var source = _priorityQueue.Dequeue();
            var res = source.Current;
            
            try {
                if(await source.MoveNextAsync() && source.Current != null){
                    _priorityQueue.Enqueue(source, source.Current);
                }
            }
            catch (Exception ex) {
                throw new MergeSortException("Error during merge operation", ex);
            }
            
            if (res != null)
                yield return res;
        }
    }
}

public class MergeSortException : Exception
{
    public MergeSortException(string message, Exception innerException = null) 
        : base(message, innerException)
    {
    }
}
