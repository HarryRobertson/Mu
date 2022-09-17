namespace Mu.Core.Extensions;

public static class ConsumerExtensions
{
    public static MuApplicationBuilder AddConsumer(this MuApplicationBuilder builder, Action<IServiceProvider, ChannelWriter<Consumed>> Consume)
    {   
        return builder.AddConsumer((sp, w, ct) =>
        {
            Consume(sp, w);
            return Task.CompletedTask;
        });
    }

    public static MuApplicationBuilder AddConsumer(this MuApplicationBuilder builder, Func<IServiceProvider, ChannelWriter<Consumed>, CancellationToken, Task> ConsumeAsync)
    {
        builder.Services.AddSingleton<IConsumer>(sp => new ConfigurableConsumer(sp)
        {
            Consumer = ConsumeAsync,
        });
        return builder;
    }
}
