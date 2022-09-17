namespace Mu.Core;

public interface IConsumer
{
    Task ConsumeAsync(ChannelWriter<Consumed> writer, CancellationToken cancellationToken = default!);
}

public abstract class Consumer : IConsumer
{
    public abstract Task ConsumeAsync(ChannelWriter<Consumed> writer, CancellationToken cancellationToken = default!);
}

internal sealed class ConfigurableConsumer : Consumer
{
    private readonly IServiceProvider serviceProvider;

    public ConfigurableConsumer(IServiceProvider serviceProvider) => this.serviceProvider = serviceProvider;

    public Func<IServiceProvider, ChannelWriter<Consumed>, CancellationToken, Task> Consumer { get; set; } = default!;

    public override Task ConsumeAsync(ChannelWriter<Consumed> writer, CancellationToken cancellationToken = default)
        => Consumer?.Invoke(serviceProvider, writer, cancellationToken) 
        ?? throw new NullReferenceException($"No {nameof(Consumer)} was configured.");
}

public sealed class Consumed
{
    public Consumed(object inner) => this.Inner = inner;

    public object Inner { get; }
}
