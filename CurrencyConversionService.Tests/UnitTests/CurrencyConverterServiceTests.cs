using CurrencyConversionService.Controllers;
using CurrencyConversionService.Helpers;
using CurrencyConversionService.Interfaces;
using CurrencyConversionService.Models.Dto.In;
using CurrencyConversionService.Models.Dto.Out;
using CurrencyConversionService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using RichardSzalay.MockHttp;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace CurrencyConversionService.Tests.UnitTests
{
    public class CurrencyConverterServiceTests
    {
        private readonly IMemoryCache _cache;
        private readonly MockHttpMessageHandler _httpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<ILogger<CurrencyConverterService>> _loggerMock;
        private readonly CurrencyConverterService _service;

        public CurrencyConverterServiceTests()
        {
            _cache = new MemoryCache(new MemoryCacheOptions());
            _httpMessageHandler = new MockHttpMessageHandler();
            _httpClient = new HttpClient(_httpMessageHandler);
            _configurationMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<CurrencyConverterService>>();
            _service = new CurrencyConverterService(_cache, _httpClient, _configurationMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task ConvertAsync_Success()
        {
            // Arrange
            var fromCurrency = "USD";
            var toCurrency = "EUR";
            var amount = 100m;
            var rates = new Dictionary<string, decimal>
            {
                { "USD", 1.2m },
                { "EUR", 1m }
            };
            _cache.Set("Rates", rates, TimeSpan.FromHours(24));

            // Act
            var result = await _service.ConvertAsync(fromCurrency, toCurrency, amount);

            // Assert
            Assert.Equal(83.33m, result, 2);
        }

        [Fact]
        public async Task ConvertAsync_CacheFallback()
        {
            // Arrange
            var fromCurrency = "USD";
            var toCurrency = "EUR";
            var amount = 100m;

            var xmlResponse = @"<gesmes:Envelope xmlns:gesmes='http://www.gesmes.org/xml/2002-08-01' xmlns='http://www.ecb.int/vocabulary/2002-08-01/eurofxref'>
            <Cube>
                <Cube time='2023-07-20'>
                    <Cube currency='USD' rate='1.2'/>
                    <Cube currency='EUR' rate='1'/>
                </Cube>
            </Cube>
            </gesmes:Envelope>";

            _httpMessageHandler.When("http://example.com").Respond("application/xml", xmlResponse);
            _configurationMock.Setup(c => c["CurrencyConverter:EcbRatesUrl"]).Returns("http://example.com");

            // Act
            var result = await _service.ConvertAsync(fromCurrency, toCurrency, amount);

            // Assert
            Assert.Equal(83.33m, result, 2);
        }

        [Fact]
        public async Task GetRatesAsync_UsesCache()
        {
            // Arrange
            var rates = new Dictionary<string, decimal>
            {
                { "USD", 1.2m },
                { "EUR", 1m }
            };
            _cache.Set("Rates", rates, TimeSpan.FromHours(24));

            // Act
            var result = await _service.GetRatesAsync();

            // Assert
            Assert.Equal(rates, result);
        }

        [Fact]
        public async Task GetRatesAsync_ForceUpdate()
        {
            // Arrange
            var xmlResponse = @"<gesmes:Envelope xmlns:gesmes='http://www.gesmes.org/xml/2002-08-01' xmlns='http://www.ecb.int/vocabulary/2002-08-01/eurofxref'>
            <Cube>
                <Cube time='2023-07-20'>
                    <Cube currency='USD' rate='1.2'/>
                    <Cube currency='EUR' rate='1'/>
                </Cube>
            </Cube>
            </gesmes:Envelope>";

            _httpMessageHandler.When("http://example.com").Respond("application/xml", xmlResponse);
            _configurationMock.Setup(c => c["CurrencyConverter:EcbRatesUrl"]).Returns("http://example.com");

            // Act
            var result = await _service.GetRatesAsync(true);

            // Assert
            Assert.Equal(1.2m, result["USD"]);
            Assert.Equal(1m, result["EUR"]);
        }

        [Fact]
        public async Task ConvertAsync_ThrowsException_WhenRatesUnavailable()
        {
            // Arrange
            var fromCurrency = "USD";
            var toCurrency = "EUR";
            var amount = 100m;

            _httpMessageHandler.When("http://example.com").Throw(new HttpRequestException());
            _configurationMock.Setup(c => c["CurrencyConverter:EcbRatesUrl"]).Returns("http://example.com");

            // Act & Assert
            await Assert.ThrowsAsync<CurrencyConversionException>(() => _service.ConvertAsync(fromCurrency, toCurrency, amount));
        }

        [Fact]
        public async Task ConvertAsync_ThrowsException_ForInvalidCurrency()
        {
            // Arrange
            var fromCurrency = "INVALID";
            var toCurrency = "EUR";
            var amount = 100m;
            var rates = new Dictionary<string, decimal>
            {
                { "USD", 1.2m },
                { "EUR", 1m }
            };
            _cache.Set("Rates", rates, TimeSpan.FromHours(24));

            // Act & Assert
            await Assert.ThrowsAsync<CurrencyConversionException>(() => _service.ConvertAsync(fromCurrency, toCurrency, amount));
        }

        [Fact]
        public async Task GetRatesAsync_ThrowsException_ForInvalidXmlResponse()
        {
            // Arrange
            var invalidXmlResponse = "<invalid>";
            _httpMessageHandler.When("http://example.com").Respond("application/xml", invalidXmlResponse);
            _configurationMock.Setup(c => c["CurrencyConverter:EcbRatesUrl"]).Returns("http://example.com");

            // Act & Assert
            await Assert.ThrowsAsync<CurrencyConversionException>(() => _service.GetRatesAsync(true));
        }

        [Fact]
        public async Task GetRatesAsync_ThrowsException_WhenCacheAndServiceFail()
        {
            // Arrange
            _httpMessageHandler.When("http://example.com").Throw(new HttpRequestException());
            _configurationMock.Setup(c => c["CurrencyConverter:EcbRatesUrl"]).Returns("http://example.com");

            // Act & Assert
            await Assert.ThrowsAsync<CurrencyConversionException>(() => _service.GetRatesAsync());
        }

        [Fact]
        public async Task GetRatesAsync_CacheExpiry()
        {
            // Arrange
            var initialRates = new Dictionary<string, decimal>
            {
                { "USD", 1.2m },
                { "EUR", 1m }
            };
            _cache.Set("Rates", initialRates, TimeSpan.FromSeconds(1));

            var xmlResponse = @"<gesmes:Envelope xmlns:gesmes='http://www.gesmes.org/xml/2002-08-01' xmlns='http://www.ecb.int/vocabulary/2002-08-01/eurofxref'>
            <Cube>
                <Cube time='2023-07-20'>
                    <Cube currency='USD' rate='1.3'/>
                    <Cube currency='EUR' rate='1'/>
                </Cube>
            </Cube>
            </gesmes:Envelope>";

            _httpMessageHandler.When("http://example.com").Respond("application/xml", xmlResponse);
            _configurationMock.Setup(c => c["CurrencyConverter:EcbRatesUrl"]).Returns("http://example.com");

            // Wait for the cache to expire
            await Task.Delay(1500);

            // Act
            var result = await _service.GetRatesAsync();

            // Assert
            Assert.Equal(1.3m, result["USD"]);
            Assert.Equal(1m, result["EUR"]);
        }
    }
}
