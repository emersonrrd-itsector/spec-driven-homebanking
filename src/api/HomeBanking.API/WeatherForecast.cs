/// <summary>
/// Weather forecast record.
/// </summary>
/// <param name="Date">The date of the forecast.</param>
/// <param name="TemperatureC">Temperature in Celsius.</param>
/// <param name="Summary">Weather summary.</param>
public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    /// <summary>
    /// Gets temperature in Fahrenheit.
    /// </summary>
    public int TemperatureF => 32 + (int)(this.TemperatureC / 0.5556);
}
