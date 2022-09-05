namespace Mu.Core.Extensions;

public static class ReceiverExtensions
{
    public static MuApplicationBuilder AddReceiver(this MuApplicationBuilder builder, ReceiveEvent consume)
    {   
        return builder.AddReceiver((sp, w, ct) =>
        {
            consume(sp, w);
            return Task.CompletedTask;
        });
    }

    public static MuApplicationBuilder AddReceiver(this MuApplicationBuilder builder, AsyncReceiveEvent consumeAsync)
    {
        builder.Services.AddSingleton(consumeAsync);
        return builder;
    }
}
