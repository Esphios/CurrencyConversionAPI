using CurrencyConversionService.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CurrencyConversionService.Services
{
    public class CurrencyConverterService : ICurrencyConverterService
    {
        private readonly IMemoryCache _cache;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public CurrencyConverterService(IMemoryCache cache, HttpClient httpClient, IConfiguration configuration)
        {
            _cache = cache;
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<decimal> ConvertAsync(string fromCurrency, string toCurrency, decimal amount)
        {
            var rates = await GetRatesAsync();
            var fromRate = rates[fromCurrency];
            var toRate = rates[toCurrency];
            return amount * (toRate / fromRate);
        }

        public async Task UpdateBulkRatesAsync()
        {
            await GetRatesAsync(true);
        }

        private async Task<Dictionary<string, decimal>> GetRatesAsync(bool forceUpdate = false)
        {
            if (!forceUpdate && _cache.TryGetValue("Rates", out Dictionary<string, decimal> cachedRates))
            {
                return cachedRates;
            }

            var ecbRatesUrl = _configuration["CurrencyConverter:EcbRatesUrl"];
            var response = await _httpClient.GetStringAsync(ecbRatesUrl);
            var rates = ParseRates(response);
            _cache.Set("Rates", rates, TimeSpan.FromHours(24));
            return rates;
        }

        private static Dictionary<string, decimal> ParseRates(string xml)
        {
            var xdoc = XDocument.Parse(xml);
            var ns = xdoc.Root!.GetDefaultNamespace();
            var rates = xdoc.Descendants(ns + "Cube")
                            .Where(x => x.Attribute("currency") != null)
                            .ToDictionary(
                                x => x.Attribute("currency")!.Value,
                                x => decimal.Parse(x.Attribute("rate")!.Value));

            rates["EUR"] = 1m; // ECB rates are relative to EUR
            return rates;
        }
    }
}
