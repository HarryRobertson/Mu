namespace Mu.Core;

public delegate Task AsyncProduceEvent(IServiceProvider services, ChannelReader<MuProduced> produced, CancellationToken cancellationToken = default!);
public delegate void ProduceEvent(IServiceProvider services, ChannelReader<MuProduced> produced);
public delegate Task AsyncReceiveEvent(IServiceProvider services, ChannelWriter<MuReceived> received, CancellationToken cancellationToken = default!);
public delegate void ReceiveEvent(IServiceProvider services, ChannelWriter<MuReceived> received);

public sealed class MuApplication : IDisposable
{
    private IList<Func<MuContext, Func<Task>, Task>> handlers = new List<Func<MuContext, Func<Task>, Task>>();

    private bool running = false;

    public MuApplication(IServiceProvider services) => Services = services;

    public static MuApplicationBuilder CreateBuilder(params string[] args) => new MuApplicationBuilder(args);

    public IServiceProvider Services { get; init; } = default!;

    public MuApplication Use(Func<MuContext, Func<Task>, Task> handler)
    {
        handlers.Add(handler);
        return this;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default) 
    {
        running = true;

        var logger = Services.GetRequiredService<ILogger<MuApplication>>();

        logger.LogDebug("Building pipeline...");
        var pipeline = handlers
            .Reverse()
            .Aggregate((MuContext context) => Task.CompletedTask, 
                (next, handler) => (MuContext context) => handler(context, () => next(context)));

        logger.LogInformation("Starting receivers...");
        var receivers = Services.GetServices<AsyncReceiveEvent>()
            .Select(r => r(Services, Services.GetRequiredService<ChannelWriter<MuReceived>>(), cancellationToken))
            .ToList();
        logger.LogInformation("Starting producers...");
        var producers = Services.GetServices<AsyncProduceEvent>()
            .Select(p => p(Services, Services.GetRequiredService<ChannelReader<MuProduced>>(), cancellationToken))
            .ToList();

        logger.LogInformation("Starting host...");
        var channelReader = Services.GetRequiredService<ChannelReader<MuReceived>>();
        var channelWriter = Services.GetRequiredService<ChannelWriter<MuProduced>>();
        // var lifetime = Services.GetRequiredService<IHostApplicationLifetime>();
        while (running && await channelReader.WaitToReadAsync(cancellationToken))
        {
            await foreach (var dequeued in channelReader.ReadAllAsync())
            {
                var context = new MuContext { ConsumedEvent = dequeued, ProducedEvent = new() };
                cancellationToken.ThrowIfCancellationRequested();
                logger.LogJson(context.ConsumedEvent, LogLevel.Trace);
                await pipeline(context);
                logger.LogJson(context.ProducedEvent, LogLevel.Trace);
                cancellationToken.ThrowIfCancellationRequested();
                await channelWriter.WriteAsync(context.ProducedEvent, cancellationToken);
            }
        }
    }

    // public Task StartAsync(CancellationToken cancellationToken = default)
    // {
    //     cancellationToken.ThrowIfCancellationRequested();
    //     RunAsync(cancellationToken);
    //     return Task.CompletedTask;
    // }

    // public Task StopAsync(CancellationToken cancellationToken = default)
    // { 
    //     cancellationToken.ThrowIfCancellationRequested();
    //     running = false;
    //     return Task.CompletedTask;
    // }

    public void Dispose() { }
}
