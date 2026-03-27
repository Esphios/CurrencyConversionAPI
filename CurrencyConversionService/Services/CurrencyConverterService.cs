using CurrencyConversionService.Helpers;
using CurrencyConversionService.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CurrencyConversionService.Services;

public class CurrencyConverterService(
    IMemoryCache cache,
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<CurrencyConverterService> logger) : ICurrencyConverterService
{

    public async Task<decimal> ConvertAsync(string fromCurrency, string toCurrency, decimal amount)
    {
        try
        {
            Dictionary<string, decimal> rates = await GetRatesAsync();
            return rates.TryGetValue(fromCurrency, out decimal fromRate) && rates.TryGetValue(toCurrency, out decimal toRate)
                ? amount * (toRate / fromRate)
                : throw new CurrencyConversionException($"Failed to convert from {fromCurrency} to {toCurrency}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while converting currencies from {FromCurrency} to {ToCurrency}", fromCurrency, toCurrency);
            throw new CurrencyConversionException($"Failed to convert from {fromCurrency} to {toCurrency}", ex);
        }
    }

    public async Task UpdateBulkRatesAsync()
    {
        _ = await GetRatesAsync(true);
    }

    public async Task<Dictionary<string, decimal>> GetRatesAsync(bool forceUpdate = false)
    {
        if (!forceUpdate && cache.TryGetValue("Rates", out Dictionary<string, decimal> cachedRates))
        {
            return cachedRates;
        }

        try
        {
            string ecbRatesUrl = configuration["CurrencyConverter:EcbRatesUrl"];
            string response = await httpClient.GetStringAsync(ecbRatesUrl);
            Dictionary<string, decimal> rates = ParseRates(response);
            _ = cache.Set("Rates", rates, TimeSpan.FromHours(24));
            return rates;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while fetching or parsing rates from ECB");
            return cache.TryGetValue("Rates", out Dictionary<string, decimal> fallbackRates)
                ? fallbackRates
                : throw new CurrencyConversionException("Failed to fetch or parse rates from ECB", ex);
        }
    }

    public static Dictionary<string, decimal> ParseRates(string xml)
    {
        XDocument xdoc = XDocument.Parse(xml);
        XNamespace ns = xdoc.Root!.GetDefaultNamespace();
        Dictionary<string, decimal> rates = xdoc.Descendants(ns + "Cube")
                        .Where(x => x.Attribute("currency") != null)
                        .ToDictionary(
                            x => x.Attribute("currency")!.Value,
                            x => decimal.Parse(x.Attribute("rate")!.Value.Replace(".", ",")));

        rates["EUR"] = 1m; // ECB rates are relative to EUR
        return rates;
    }
}
