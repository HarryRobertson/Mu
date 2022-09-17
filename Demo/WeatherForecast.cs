namespace Mu.Demo;

public record class WeatherForecast
{
    public DateTime Date { get; set; }

    public int TemperatureC { get; set; }

    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    public string? Summary { get; set; }
}

public record class WeatherRequest 
{
    public int Days { get; set; }
}
