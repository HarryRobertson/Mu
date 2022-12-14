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
        logger.LogInformation($"MU_ENVIRONMENT={Environment.GetEnvironmentVariable("MU_ENVIRONMENT")}");

        logger.LogDebug("Building pipeline...");
        var pipeline = Handlers
            .Reverse()
            .Aggregate((MuContext context) => Task.CompletedTask,
                (next, handler) => (MuContext context) => handler(context, () => next(context)));

        logger.LogInformation("Starting consumers...");
        var consumers = Services.GetServices<IConsumer>()
            .Select(r =>
            {
                var writer = Services.CreateScope().ServiceProvider.GetRequiredService<IMessageWriter>();
                return r.ConsumeAsync(writer, cancellationToken);
            })
            .ToList();

        logger.LogInformation("Starting producers...");
        var producers = Services.GetServices<IProducer>()
            .Select(p =>
            {
                var reader = Services.CreateScope().ServiceProvider.GetRequiredService<IMessageReader>();
                return p.ProduceAsync(reader, cancellationToken);
            })
            .ToList();

        logger.LogInformation("Starting host...");
        var consumedReader = Services.GetRequiredService<ChannelReader<Consumed>>();
        var producedWriter = Services.GetRequiredService<ChannelWriter<Produced>>();

        while (running && await consumedReader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
        {
            await foreach (var dequeued in consumedReader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
            {
                try
                {
                    var context = new MuContext(dequeued.Inner);

                    logger.LogJson(context.Consumed, LogLevel.Trace);

                    cancellationToken.ThrowIfCancellationRequested();
                    await pipeline(context).ConfigureAwait(false);
                    
                    logger.LogJson(context.Produced, LogLevel.Trace);

                    cancellationToken.ThrowIfCancellationRequested();
                    var success = context.Status is Status.Success;
                    await dequeued.CompleteAsync(success).ConfigureAwait(false);
                    if (success)
                    {
                        await producedWriter.WriteAsync(new(context.Produced), cancellationToken).ConfigureAwait(false);
                    }
                }
                catch
                {
                    await dequeued.CompleteAsync(false).ConfigureAwait(false);
                }
            }
        }
    }

    public void Dispose() { }
}
