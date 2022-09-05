namespace Mu.Core.Extensions;

public static class RunExtensions
{
    public static void Run(this MuApplication app, Func<MuContext, Task> handler)
        => app.Use((MuContext context, Func<Task> _) => handler(context));
}
