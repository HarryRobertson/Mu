using System.Text;
using System.Text.Json;

var builder = MuApplication.CreateBuilder(args);

builder.AddConsumer((sp, w) => 
{
    var channel = sp.GetRequiredService<IModel>();
    channel.QueueDeclare(queue: "foo", durable: false, exclusive: false, autoDelete: false, arguments: null);
    var consumer = new EventingBasicConsumer(channel);
    consumer.Received += (model, ea) => 
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        if (JsonSerializer.Deserialize<WeatherRequest>(message) is {} temp)
        {
            w.TryWrite(new Consumed(temp));
        }
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
                var content = JsonSerializer.Serialize(read.Inner);
                var body = Encoding.UTF8.GetBytes(content);
                channel.BasicPublish(exchange: "", routingKey: "bar", basicProperties: null, body);
            }
        }
    }
});

builder.Services.AddSingleton<IConnectionFactory>(new ConnectionFactory { HostName = "localhost" });
builder.Services.AddScoped(sp => sp.GetRequiredService<IConnectionFactory>().CreateConnection());
builder.Services.AddScoped(sp => sp.GetRequiredService<IConnection>().CreateModel());

string[] Summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

var app = builder.Build();

app.Map<WeatherRequest>(map =>
{
    map.Run(context => 
    {
        var request = (WeatherRequest)context.Consumed;
        context.Produced = Enumerable.Range(1, request.Days).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
        return Task.CompletedTask;
    });
});

await app.RunAsync();
