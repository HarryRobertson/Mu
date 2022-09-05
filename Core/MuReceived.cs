namespace Mu.Core;

public sealed class MuReceived
{
    public IDictionary<string, string> Headers { get; init; } = new Headers();

    public object Content { get; init; } = default!;
}
