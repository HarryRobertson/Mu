namespace Mu.Core;

public sealed class MuContext
{
    public object Consumed { get; init; } = default!;
    public object Produced { get; set; } = default!;
}
