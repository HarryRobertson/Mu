using System.Text;

var builder = MuApplication.CreateBuilder(args);

builder.AddConsumer((sp, w, ct) =>
{
    var channel = sp.GetRequiredService<IModel>();
    channel.QueueDeclare(queue: "foo", durable: false, exclusive: false, autoDelete: false, arguments: null);
    var consumer = new EventingBasicConsumer(channel);
    
    var completeAsync = (bool success, ulong tag) =>
    {
        if (success)
            channel.BasicAck(tag, multiple: false);
        else
            channel.BasicReject(tag, requeue: true);
        return Task.CompletedTask;
    };

    consumer.Received += async (_, ea) =>
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        await w.WriteAsync<WeatherRequest>(message, s => completeAsync(s, ea.DeliveryTag), ct);
    };
    
    channel.BasicConsume(queue: "foo", autoAck: false, consumer: consumer);
    return Task.CompletedTask;
});

builder.AddProducer(async (sp, reader, ct) =>
{
    var channel = sp.GetRequiredService<IModel>();
    channel.QueueDeclare(queue: "bar", durable: false, exclusive: false, autoDelete: false, arguments: null);
    while (!ct.IsCancellationRequested)
    {
        var read = await reader.ReadAsync(ct);
        var body = Encoding.UTF8.GetBytes(read);
        channel.BasicPublish(exchange: "", routingKey: "bar", basicProperties: null, body);
    }
});

builder.Services.AddSingleton<IConnectionFactory>(_ => new ConnectionFactory { HostName = "localhost", Port = 5672 });
builder.Services.AddScoped(sp => sp.GetRequiredService<IConnectionFactory>().CreateConnection());
builder.Services.AddScoped(sp => sp.GetRequiredService<IConnection>().CreateModel());

string[] Summaries = 
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
