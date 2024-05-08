using Diginsight.Strings;

namespace SampleWebApi
{
    public class WeatherForecast
    {
        [LogStringableMember(Order = 1)]
        public DateOnly Date { get; set; }

        [LogStringableMember(Order = 2)]
        public int TemperatureC { get; set; }

        [NonLogStringableMember]
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        [LogStringableMember(Order = 3)]
        public string? Summary { get; set; }
    }
}
