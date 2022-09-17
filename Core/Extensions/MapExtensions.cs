namespace Mu.Core.Extensions;

public static class MapExtensions 
{
    public static MuApplication Map<T>(this MuApplication app, Func<MuContext, Func<Task>, Task> handler)
    {
        var predicatedHandler = (MuContext context, Func<Task> next) 
            => context.Consumed switch
            {
                T => handler(context, next),
                _ => next()
            };
        app.Use(predicatedHandler);
        return app;
    }

    public static MuApplication Map<T>(this MuApplication app, Action<MuApplication> middleware)
    {
        var temp = new MuApplication(app.Services);
        middleware(temp);
        foreach(var handler in temp.Handlers)
        {
            app.Map<T>(handler);
        }
        return app;
    }

    // public static MuApplication Map(this MuApplication app, Type eventType, Func<MuContext, Func<Task>, Task> handler) 
    //     => app.Map(eventType.FullName!, handler);

    // public static MuApplication Map<T>(this MuApplication app, Func<MuContext, Func<Task>, Task> handler)
    //     => app.Map(typeof(T).FullName!, handler);

    // public static MuApplication Map(this MuApplication app, string eventType, Func<MuApplication, MuApplication> handlers)
    // {

    // }

    // public static MuApplication MapHandlers(this MuApplication app)
    // {
    //     foreach (var type in typeof(MuApplication).Assembly.GetTypes())
    //     {
    //         type.Attributes.
    //     }
    // }
}
