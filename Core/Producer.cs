namespace Mu.Core;

public interface IProducer
{
    Task ProduceAsync(Func<CancellationToken, Task<object>> reader, CancellationToken cancellationToken = default!);
}

public abstract class Producer : IProducer
{
    public abstract Task ProduceAsync(Func<CancellationToken, Task<object>> reader, CancellationToken cancellationToken = default!);
}

internal sealed class ConfigurableProducer : Producer
{
    private readonly IServiceProvider serviceProvider;

    public ConfigurableProducer(IServiceProvider serviceProvider) => this.serviceProvider = serviceProvider;

    public Func<IServiceProvider, Func<CancellationToken, Task<object>>, CancellationToken, Task> Producer { get; set; } = default!;

    public override Task ProduceAsync(Func<CancellationToken, Task<object>> reader, CancellationToken cancellationToken = default)
        => Producer?.Invoke(serviceProvider, reader, cancellationToken) 
        ?? throw new NullReferenceException($"No {nameof(Producer)} was configured.");
}
