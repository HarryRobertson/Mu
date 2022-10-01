namespace Mu.Core;

internal sealed class Consumed
{
    public Consumed(object inner, Func<bool, Task> completeAsync) 
    {
        this.Inner = inner;
        this.CompleteAsync = completeAsync;
    }

    public object Inner { get; }
    public Func<bool, Task> CompleteAsync { get; }
}
