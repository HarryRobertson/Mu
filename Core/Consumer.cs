using System.Text.Json;

namespace Mu.Core;

public interface IMessageWriter
{
    Task WriteAsync<T>(string message, Func<bool, Task> completeMessage, CancellationToken cancellationToken = default);
}

internal class MessageWriter : IMessageWriter
{
    private readonly ChannelWriter<Consumed> consumedWriter;

    public MessageWriter(ChannelWriter<Consumed> consumedWriter) => this.consumedWriter = consumedWriter;

    public async Task WriteAsync<T>(string message, Func<bool, Task> completeMessage, CancellationToken cancellationToken = default)
    {
        if (JsonSerializer.Deserialize<T>(message) is { } temp)
        {
            await consumedWriter.WriteAsync(new(temp, completeMessage), cancellationToken);
        }
    }
}

public interface IConsumer
{
    Task ConsumeAsync(IMessageWriter writer, CancellationToken cancellationToken = default!);
}

public abstract class Consumer : IConsumer
{
    public abstract Task ConsumeAsync(IMessageWriter writer, CancellationToken cancellationToken = default!);
}

internal sealed class ConfigurableConsumer : Consumer
{
    private readonly IServiceProvider serviceProvider;

    public ConfigurableConsumer(IServiceProvider serviceProvider) => this.serviceProvider = serviceProvider;

    public Func<IServiceProvider, IMessageWriter, CancellationToken, Task> Consumer { get; set; } = default!;

    public override Task ConsumeAsync(IMessageWriter writer, CancellationToken cancellationToken = default)
        => Consumer?.Invoke(serviceProvider, writer, cancellationToken) 
        ?? throw new NullReferenceException($"No {nameof(Consumer)} was configured.");
}
