namespace Mu.Core;

public sealed class MuProduced 
{
    public IDictionary<string, string> Headers { get; } = new Headers();

    public object Content { get; set; } = default!;
}
