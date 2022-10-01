namespace Mu.Core;

public interface IConsumer
{
    Task ConsumeAsync(Func<object, Func<bool, Task>, CancellationToken, Task> writer, CancellationToken cancellationToken = default!);
}

public abstract class Consumer : IConsumer
{
    public abstract Task ConsumeAsync(Func<object, Func<bool, Task>, CancellationToken, Task> writer, CancellationToken cancellationToken = default!);
}

internal sealed class ConfigurableConsumer : Consumer
{
    private readonly IServiceProvider serviceProvider;

    public ConfigurableConsumer(IServiceProvider serviceProvider) => this.serviceProvider = serviceProvider;

    public Func<IServiceProvider, Func<object, Func<bool, Task>, CancellationToken, Task>, CancellationToken, Task> Consumer { get; set; } = default!;

    public override Task ConsumeAsync(Func<object, Func<bool, Task>, CancellationToken, Task> writer, CancellationToken cancellationToken = default)
        => Consumer?.Invoke(serviceProvider, writer, cancellationToken) 
        ?? throw new NullReferenceException($"No {nameof(Consumer)} was configured.");
}
