namespace Mu.Core;

internal sealed class Produced
{
    public Produced(object inner) => this.Inner = inner;

    public object Inner { get; }
}
