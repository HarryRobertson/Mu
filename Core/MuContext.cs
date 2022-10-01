namespace Mu.Core;

public sealed class MuContext
{
    public MuContext(object consumed) => this.Consumed =  consumed;
    public object Consumed { get; init; } = default!;
    public object Produced { get; set; } = default!;
    public Status Status { get; set; }
}

public enum Status
{
    Success,
    Failure,
}
