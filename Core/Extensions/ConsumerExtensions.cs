namespace Mu.Core.Extensions;

public static class ConsumerExtensions
{
    public static MuApplicationBuilder AddConsumer(this MuApplicationBuilder builder, Action<IServiceProvider, IMessageWriter> Consume)
    {   
        return builder.AddConsumer((sp, w, ct) =>
        {
            Consume(sp, w);
            return Task.CompletedTask;
        });
    }

    public static MuApplicationBuilder AddConsumer(this MuApplicationBuilder builder, Func<IServiceProvider, IMessageWriter, CancellationToken, Task> ConsumeAsync)
    {
        builder.Services.AddSingleton<IConsumer>(sp => new ConfigurableConsumer(sp)
        {
            Consumer = ConsumeAsync,
        });
        return builder;
    }
}
