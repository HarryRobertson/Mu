namespace Mu.Core.Extensions;

public static class ProducerExtensions
{
    public static MuApplicationBuilder AddProducer(this MuApplicationBuilder builder, ProduceEvent produce)
    {
        return builder.AddProducer((sp, r, ct) =>
        {
            produce(sp, r);
            return Task.CompletedTask;
        });
    }

    public static MuApplicationBuilder AddProducer(this MuApplicationBuilder builder, AsyncProduceEvent produceAsync)
    {
        builder.Services.AddSingleton(produceAsync);
        return builder;
    }
}
