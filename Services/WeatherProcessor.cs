using DisasterAPI.Interfaces;
using DisasterAPI.Models;

namespace DisasterAPI.Services;

public class WeatherProcessor : IWeatherProcessor
{
    private readonly IWeatherDataProvider _dataProvider;

    public WeatherProcessor(IWeatherDataProvider dataProvider)
    {
        _dataProvider = dataProvider;
    }

    public WeatherForecast[] GenerateForecasts(int days)
    {
        try
        {
            // Another layer of try-catch in the middle tier
            var summaries = _dataProvider.GetWeatherSummaries();
            
            var forecasts = Enumerable.Range(1, days).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    _dataProvider.GetRandomTemperature(),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
                .ToArray();

            // Artificially introduce potential for exceptions
            if (forecasts.Length == 0)
            {
                throw new InvalidOperationException("No forecasts could be generated");
            }
            
            return forecasts;
        }
        catch (Exception ex)
        {
            // Bad practice: catching general exception and rethrowing
            Console.WriteLine($"Error in forecast processing: {ex.Message}");
            throw; // This will propagate to the service layer
        }
    }
}
