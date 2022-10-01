namespace Mu.Core;

public sealed class MuContext
{
    public MuContext(object consumed) => this.Consumed =  consumed;
    public object Consumed { get; init; } = default!;
    public object Produced { get; set; } = default!;
}
