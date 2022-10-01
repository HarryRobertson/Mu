namespace Mu.Core.Extensions;

public static class ProducerExtensions
{
    public static MuApplicationBuilder AddProducer(this MuApplicationBuilder builder, Action<IServiceProvider, Func<CancellationToken, Task<object>>> producer)
    {
        return builder.AddProducer((sp, r, ct) =>
        {
            producer(sp, r);
            return Task.CompletedTask;
        });
    }

    public static MuApplicationBuilder AddProducer(this MuApplicationBuilder builder, Func<IServiceProvider, Func<CancellationToken, Task<object>>, CancellationToken, Task> produceAsync)
    {
        builder.Services.AddSingleton<IProducer>(sp => new ConfigurableProducer(sp)
        {
            Producer = produceAsync,
        });
        return builder;
    }
}
