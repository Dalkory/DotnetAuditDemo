using System.Net.Http.Json;

namespace DotnetAuditDemo.Api.Clients;

public sealed class ExchangeRateClient(HttpClient httpClient)
{
    public async Task<decimal> GetEurRateAsync()
    {
        var response = await httpClient.GetFromJsonAsync<ExchangeRateResponse>(
            "https://api.frankfurter.app/latest?from=USD&to=EUR");

        return response?.Rates.GetValueOrDefault("EUR") ?? 1m;
    }

    private sealed record ExchangeRateResponse(Dictionary<string, decimal> Rates);
}
