using System.Text;
using System.Text.Json;

var builder = MuApplication.CreateBuilder(args);

builder.AddReceiver((sp, w) => 
{
    var channel = sp.GetRequiredService<IModel>();
    channel.QueueDeclare(queue: "foo", durable: false, exclusive: false, autoDelete: false, arguments: null);
    var consumer = new EventingBasicConsumer(channel);
    consumer.Received += (model, ea) => 
    {
        var body = ea.Body.ToArray();
        var jwt = Encoding.UTF8.GetString(body);
        var parts = jwt.Split('.');
        var headers = JsonSerializer.Deserialize<IDictionary<string, string>>(parts[0])
            ?? new Dictionary<string, string>();
        var content = JsonSerializer.Deserialize<object>(parts[1]) ?? new();
        var message = new MuReceived { Headers = headers, Content = content };
        w.TryWrite(message);
    };
    channel.BasicConsume(queue: "foo", autoAck: true, consumer);
});

builder.AddProducer(async (sp, r, ct) => 
{
    var channel = sp.GetRequiredService<IModel>();
    channel.QueueDeclare(queue: "bar", durable: false, exclusive: false, autoDelete: false, arguments: null);
    while (!ct.IsCancellationRequested)
    {
        if (await r.WaitToReadAsync(ct))
        {
            await foreach (var read in r.ReadAllAsync(ct))
            {
                var headers = JsonSerializer.Serialize(read.Headers);
                var content = JsonSerializer.Serialize(read.Content);
                var jwt = $"{headers}.{content}";
                var body = Encoding.UTF8.GetBytes(jwt);
                channel.BasicPublish(exchange: "", routingKey: "bar", basicProperties: null, body);
            }
        }
    }
});

builder.Services.AddSingleton<IConnectionFactory>(new ConnectionFactory { HostName = "localhost" });
builder.Services.AddScoped(sp => sp.GetRequiredService<IConnectionFactory>().CreateConnection());
builder.Services.AddScoped(sp => sp.GetRequiredService<IConnection>().CreateModel());

var app = builder.Build();

app.Use((context, next) =>
{
    // Console.WriteLine(context.ConsumedEvent.Content.ToString());
    return next();
})
.Run(context => 
{
    context.ProducedEvent.Content = context.ConsumedEvent.Content;
    return Task.CompletedTask;
});

await app.RunAsync();
