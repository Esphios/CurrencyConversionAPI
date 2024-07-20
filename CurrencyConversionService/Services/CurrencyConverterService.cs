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

namespace CurrencyConversionService.Services
{
    public class CurrencyConverterService : ICurrencyConverterService
    {
        private readonly IMemoryCache _cache;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CurrencyConverterService> _logger;

        public CurrencyConverterService(IMemoryCache cache, HttpClient httpClient, IConfiguration configuration, ILogger<CurrencyConverterService> logger)
        {
            _cache = cache;
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<decimal> ConvertAsync(string fromCurrency, string toCurrency, decimal amount)
        {
            try
            {
                // Ensure we are using cached rates if available
                var rates = await GetRatesAsync();
                if (rates.TryGetValue(fromCurrency, out decimal fromRate) && rates.TryGetValue(toCurrency, out decimal toRate))
                {
                    return amount * (toRate / fromRate);
                }
                else
                {
                    throw new CurrencyConversionException($"Failed to convert from {fromCurrency} to {toCurrency}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while converting currencies from {FromCurrency} to {ToCurrency}", fromCurrency, toCurrency);
                throw new CurrencyConversionException($"Failed to convert from {fromCurrency} to {toCurrency}", ex);
            }
        }

        public async Task UpdateBulkRatesAsync()
        {
            await GetRatesAsync(true);
        }

        public async Task<Dictionary<string, decimal>> GetRatesAsync(bool forceUpdate = false)
        {
            if (!forceUpdate && _cache.TryGetValue("Rates", out Dictionary<string, decimal> cachedRates))
            {
                return cachedRates;
            }

            try
            {
                var ecbRatesUrl = _configuration["CurrencyConverter:EcbRatesUrl"];
                var response = await _httpClient.GetStringAsync(ecbRatesUrl);
                var rates = ParseRates(response);
                _cache.Set("Rates", rates, TimeSpan.FromHours(24));
                return rates;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching or parsing rates from ECB");
                if (_cache.TryGetValue("Rates", out Dictionary<string, decimal> fallbackRates))
                {
                    return fallbackRates;
                }
                throw new CurrencyConversionException("Failed to fetch or parse rates from ECB", ex);
            }
        }

        public static Dictionary<string, decimal> ParseRates(string xml)
        {
            var xdoc = XDocument.Parse(xml);
            var ns = xdoc.Root!.GetDefaultNamespace();
            var rates = xdoc.Descendants(ns + "Cube")
                            .Where(x => x.Attribute("currency") != null)
                            .ToDictionary(
                                x => x.Attribute("currency")!.Value,
                                x => decimal.Parse(x.Attribute("rate")!.Value.Replace(".", ",")));

            rates["EUR"] = 1m; // ECB rates are relative to EUR
            return rates;
        }
    }
}
