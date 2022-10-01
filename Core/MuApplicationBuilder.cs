using System.Reflection;

namespace Mu.Core.Builder;

public sealed class MuApplicationBuilder
{
    private readonly IConfigurationBuilder configurationBuilder; 
    internal MuApplicationBuilder(params string[] args) 
    {
        var environment = Environment.GetEnvironmentVariable("MU_ENVIRONMENT");

        configurationBuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
            .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true, reloadOnChange: true)
            .AddEnvironmentVariables("MU_")
            .AddCommandLine(args)
            .AddInMemoryCollection();

        Action<ILoggingBuilder> loggingBuilder = builder => builder
            .ClearProviders()
                .AddConfiguration(configurationBuilder.Build().GetSection("Logging"))
                .AddConsole()
                .AddDebug();

        Services = new ServiceCollection()
            .AddSingleton<IConfiguration>(_ => configurationBuilder.Build())
            .AddLogging(loggingBuilder)
            .AddSingleton(Channel.CreateUnbounded<Consumed>())
            .AddSingleton(p => p.GetRequiredService<Channel<Consumed>>().Reader)
            .AddSingleton(p => p.GetRequiredService<Channel<Consumed>>().Writer)
            .AddSingleton(Channel.CreateUnbounded<Produced>())
            .AddSingleton(p => p.GetRequiredService<Channel<Produced>>().Reader)
            .AddSingleton(p => p.GetRequiredService<Channel<Produced>>().Writer)
            .AddSingleton<MuApplication>();
    }

    public MuApplicationBuilder Configure(Action<IConfigurationBuilder> configure)
    {
        configure(configurationBuilder);
        return this;
    } 

    public MuApplicationBuilder ConfigureLogging(Action<ILoggingBuilder> configure)
    {
        Services.AddLogging(configure);
        return this;
    }

    public MuApplicationBuilder ConfigureServices(Action<IServiceCollection> configure)
    {
        configure(Services);
        return this;
    }

    public IServiceCollection Services { get; } = default!;

    public MuApplication Build() 
        => Services.BuildServiceProvider()
        .GetRequiredService<MuApplication>();
}
