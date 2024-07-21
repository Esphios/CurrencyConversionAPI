using CurrencyConversionService.Controllers;
using CurrencyConversionService.Interfaces;
using CurrencyConversionService.Models.Dto.In;
using CurrencyConversionService.Models.Dto.Out;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CurrencyConversionService.Tests.UnitTests
{
    public class CurrencyConverterControllerTests
    {
        private readonly Mock<ICurrencyConverterService> _serviceMock;
        private readonly CurrencyConverterController _controller;

        public CurrencyConverterControllerTests()
        {
            _serviceMock = new Mock<ICurrencyConverterService>();
            _controller = new CurrencyConverterController(_serviceMock.Object);
        }

        [Fact]
        public async Task BulkConvert_ReturnsConvertedAmounts()
        {
            // Arrange
            var conversionRequests = new List<ConversionIn>
            {
                new() { FromCurrency = "USD", ToCurrency = "EUR", Amount = 100m },
                new() { FromCurrency = "GBP", ToCurrency = "USD", Amount = 200m }
            };

            _serviceMock.Setup(s => s.ConvertAsync("USD", "EUR", 100m)).ReturnsAsync(83.33m);
            _serviceMock.Setup(s => s.ConvertAsync("GBP", "USD", 200m)).ReturnsAsync(280m);

            // Act
            var result = await _controller.BulkConvert(conversionRequests) as OkObjectResult;
            var conversions = result!.Value as List<ConversionOut>;

            // Assert
            Assert.Equal(2, conversions!.Count);
            Assert.Equal(83.33m, conversions[0].ConvertedAmount);
            Assert.Equal(280m, conversions[1].ConvertedAmount);
        }

        [Fact]
        public async Task BulkConvert_ReturnsBadRequest_ForInvalidCurrency()
        {
            // Arrange
            var conversionRequests = new List<ConversionIn>
            {
                new() { FromCurrency = "INVALID", ToCurrency = "EUR", Amount = 100m }
            };

            // Act
            var result = await _controller.BulkConvert(conversionRequests) as BadRequestObjectResult;

            // Assert
            Assert.Equal("Invalid currencies: INVALID. Please use valid currency codes.", result!.Value);
        }

        [Fact]
        public async Task UpdateRates_ReturnsNoContent()
        {
            // Act
            var result = await _controller.UpdateRates() as NoContentResult;

            // Assert
            Assert.NotNull(result);
        }
    }
}
