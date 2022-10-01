using System.Text.Json;

namespace Mu.Core;

public interface IMessageReader
{
    Task<string> ReadAsync(CancellationToken cancellationToken = default);
}

internal class MessageReader : IMessageReader
{
    private readonly ChannelReader<Produced> producedReader;

    public MessageReader(ChannelReader<Produced> producedReader) => this.producedReader = producedReader;

    public async Task<string> ReadAsync(CancellationToken cancellationToken = default)
    {
        var read = await producedReader.WaitToReadAsync(cancellationToken)
            ? await producedReader.ReadAsync(cancellationToken).AsTask().ContinueWith(t => t.Result.Inner)
            : null;
        return JsonSerializer.Serialize(read);
    }
}

public interface IProducer
{
    Task ProduceAsync(IMessageReader reader, CancellationToken cancellationToken = default!);
}

public abstract class Producer : IProducer
{
    public abstract Task ProduceAsync(IMessageReader reader, CancellationToken cancellationToken = default!);
}

internal sealed class ConfigurableProducer : Producer
{
    private readonly IServiceProvider serviceProvider;

    public ConfigurableProducer(IServiceProvider serviceProvider) => this.serviceProvider = serviceProvider;

    public Func<IServiceProvider, IMessageReader, CancellationToken, Task> Producer { get; set; } = default!;

    public override Task ProduceAsync(IMessageReader reader, CancellationToken cancellationToken = default)
        => Producer?.Invoke(serviceProvider, reader, cancellationToken) 
        ?? throw new NullReferenceException($"No {nameof(Producer)} was configured.");
}
