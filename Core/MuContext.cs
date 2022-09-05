namespace Mu.Core;

public abstract class MuContext<TIn, TOut>
{
    public TIn ConsumedEvent { get; init; } = default!;
    public TOut ProducedEvent { get; init; } = default!;
}

public sealed class MuContext : MuContext<MuReceived, MuProduced>
{
}
