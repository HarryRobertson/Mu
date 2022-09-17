namespace Mu.Core.Builder;

public sealed class MuApplicationBuilder
{
    private readonly IConfigurationBuilder configurationBuilder; 

    internal MuApplicationBuilder(params string[] args) 
    {
        var mu_args = args
            .Select(a => a.Split('='))
            .ToDictionary(s => s.First(), s => s.Last());
        var env = mu_args.TryGetValue("MU_ENVIRONMENT", out var e) ? e : "Development";

        configurationBuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .AddInMemoryCollection();

        Services = new ServiceCollection()
            .AddSingleton<IConfiguration>(_ => configurationBuilder.Build())
            .AddSingleton(Channel.CreateUnbounded<Consumed>())
            .AddSingleton(p => p.GetRequiredService<Channel<Consumed>>().Reader)
            .AddSingleton(p => p.GetRequiredService<Channel<Consumed>>().Writer)
            .AddSingleton(Channel.CreateUnbounded<Produced>())
            .AddSingleton(p => p.GetRequiredService<Channel<Produced>>().Reader)
            .AddSingleton(p => p.GetRequiredService<Channel<Produced>>().Writer)
            .AddLogging(logging => logging
                .ClearProviders()
                .AddConfiguration(configurationBuilder.Build().GetSection("Logging"))
                .AddConsole()
                .AddDebug())
            .AddSingleton<MuApplication>();
    }

    public IServiceCollection Services { get; } = default!;

    // public MuApplicationBuilder ConfigureLogging(Action<IServiceProvider, ILoggingBuilder> configure)
    // {
    //     Services.AddLogging(logging => 
    //     {
    //         var injected = (ILoggingBuilder builder) => configure(Services.BuildServiceProvider(), builder);
    //         injected(logging);
    //     });
    //     return this;
    // }

    public MuApplication Build() 
        => Services.BuildServiceProvider()
        .GetRequiredService<MuApplication>();
}
