
using Diginsight.Stringify;

namespace SampleBlazorWebAppPerPage
{
    public class WeatherForecast
    {
        [StringifiableMember(Order = 1)]
        public DateOnly Date { get; set; }

        [StringifiableMember(Order = 2)]
        public int TemperatureC { get; set; }

        [NonStringifiableMember]
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        [StringifiableMember(Order = 3)]
        public string? Summary { get; set; }
    }
}
