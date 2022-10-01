namespace Mu.Core.Extensions;

public static class ProducerExtensions
{
    public static MuApplicationBuilder AddProducer(this MuApplicationBuilder builder, Action<IServiceProvider, IMessageReader> producer)
    {
        return builder.AddProducer((sp, r, ct) =>
        {
            producer(sp, r);
            return Task.CompletedTask;
        });
    }

    public static MuApplicationBuilder AddProducer(this MuApplicationBuilder builder, Func<IServiceProvider, IMessageReader, CancellationToken, Task> produceAsync)
    {
        builder.Services.AddSingleton<IProducer>(sp => new ConfigurableProducer(sp)
        {
            Producer = produceAsync,
        });
        return builder;
    }
}
