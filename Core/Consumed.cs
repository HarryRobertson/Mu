namespace Mu.Core;

internal sealed class Consumed
{
    public Consumed(object inner) => this.Inner = inner;

    public object Inner { get; }
}
