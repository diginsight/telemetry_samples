using Common;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace EasySampleBlazorv2.Client.Shared
{
    public partial class SurveyPrompt : ComponentBase
    {
        [Inject] protected ILogger<SurveyPrompt> _logger { get; set; }

        [Parameter] public string Title { get; set; }

        protected override async Task OnInitializedAsync()
        {
            using var scope = _logger.BeginMethodScope();

            //scope.LogDebug($"Http.BaseAddress: {Http.BaseAddress}");
            ////forecasts = await Http.GetFromJsonAsync<WeatherForecast[]>("sample-data/weather.json");
            //forecasts = await Http.GetFromJsonAsync<WeatherForecast[]>("WeatherForecast");
            //scope.LogDebug(new { forecasts });
        }
    }
}
