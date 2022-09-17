namespace Mu.Core;

public sealed class MuApplication : IDisposable
{
    private bool running = false;

    public MuApplication(IServiceProvider services) => Services = services;

    internal IList<Func<MuContext, Func<Task>, Task>> Handlers = new List<Func<MuContext, Func<Task>, Task>>();

    public static MuApplicationBuilder CreateBuilder(params string[] args) => new MuApplicationBuilder(args);

    public IServiceProvider Services { get; init; } = default!;

    public MuApplication Use(Func<MuContext, Func<Task>, Task> handler)
    {
        Handlers.Add(handler);
        return this;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default) 
    {
        running = true;

        var logger = Services.GetRequiredService<ILogger<MuApplication>>();

        logger.LogDebug("Building pipeline...");
        var pipeline = Handlers
            .Reverse()
            .Aggregate((MuContext context) => Task.CompletedTask, 
                (next, handler) => (MuContext context) => handler(context, () => next(context)));

        logger.LogInformation("Starting Consumers...");
        var consumedWriter = Services.GetRequiredService<ChannelWriter<Consumed>>();
        var consumers = Services.GetServices<IConsumer>()
            .Select(r => r.ConsumeAsync(consumedWriter, cancellationToken))
            .ToList();

        logger.LogInformation("Starting producers...");
        var producedReader = Services.GetRequiredService<ChannelReader<Produced>>();
        var producers = Services.GetServices<IProducer>()
            .Select(r => r.ProduceAsync(producedReader, cancellationToken))
            .ToList();

        logger.LogInformation("Starting host...");
        var consumedReader = Services.GetRequiredService<ChannelReader<Consumed>>();
        var producedWriter = Services.GetRequiredService<ChannelWriter<Produced>>();
        // var lifetime = Services.GetRequiredService<IHostApplicationLifetime>();
        while (running && await consumedReader.WaitToReadAsync(cancellationToken))
        {
            await foreach (var dequeued in consumedReader.ReadAllAsync())
            {
                var context = new MuContext { Consumed = dequeued.Inner, Produced = new() };
                cancellationToken.ThrowIfCancellationRequested();
                logger.LogJson(context.Consumed, LogLevel.Trace);
                await pipeline(context);
                logger.LogJson(context.Produced, LogLevel.Trace);
                cancellationToken.ThrowIfCancellationRequested();
                await producedWriter.WriteAsync(new(context.Produced), cancellationToken);
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
