namespace Mu.Core;

public interface IProducer
{
    Task ProduceAsync(ChannelReader<Produced> reader, CancellationToken cancellationToken = default!);
}

public abstract class Producer : IProducer
{
    public abstract Task ProduceAsync(ChannelReader<Produced> reader, CancellationToken cancellationToken = default!);
}

internal sealed class ConfigurableProducer : Producer
{
    private readonly IServiceProvider serviceProvider;

    public ConfigurableProducer(IServiceProvider serviceProvider) => this.serviceProvider = serviceProvider;

    public Func<IServiceProvider, ChannelReader<Produced>, CancellationToken, Task> Producer { get; set; } = default!;

    public override Task ProduceAsync(ChannelReader<Produced> reader, CancellationToken cancellationToken = default)
        => Producer?.Invoke(serviceProvider, reader, cancellationToken) 
        ?? throw new NullReferenceException($"No {nameof(Producer)} was configured.");
}

public sealed class Produced
{
    public Produced(object inner) => this.Inner = inner;

    public object Inner { get; }
}
