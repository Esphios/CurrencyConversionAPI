using CurrencyConversionService.Controllers;
using CurrencyConversionService.Interfaces;
using CurrencyConversionService.Models.Dto.In;
using CurrencyConversionService.Models.Dto.Out;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CurrencyConversionService.Tests.UnitTests;

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
        List<ConversionIn> conversionRequests =
        [
            new() { FromCurrency = "USD", ToCurrency = "EUR", Amount = 100m },
            new() { FromCurrency = "GBP", ToCurrency = "USD", Amount = 200m }
        ];

        _ = _serviceMock.Setup(s => s.ConvertAsync("USD", "EUR", 100m)).ReturnsAsync(83.33m);
        _ = _serviceMock.Setup(s => s.ConvertAsync("GBP", "USD", 200m)).ReturnsAsync(280m);

        // Act
        OkObjectResult? result = await _controller.BulkConvert(conversionRequests) as OkObjectResult;
        List<ConversionOut>? conversions = result!.Value as List<ConversionOut>;

        // Assert
        Assert.Equal(2, conversions!.Count);
        Assert.Equal(83.33m, conversions[0].ConvertedAmount);
        Assert.Equal(280m, conversions[1].ConvertedAmount);
    }

    [Fact]
    public async Task BulkConvert_ReturnsBadRequest_ForInvalidCurrency()
    {
        // Arrange
        List<ConversionIn> conversionRequests =
        [
            new() { FromCurrency = "INVALID", ToCurrency = "EUR", Amount = 100m }
        ];

        // Act
        BadRequestObjectResult? result = await _controller.BulkConvert(conversionRequests) as BadRequestObjectResult;

        // Assert
        Assert.Equal("Invalid currencies: INVALID. Please use valid currency codes.", result!.Value);
    }

    [Fact]
    public async Task UpdateRates_ReturnsNoContent()
    {
        // Act
        NoContentResult? result = await _controller.UpdateRates() as NoContentResult;

        // Assert
        Assert.NotNull(result);
    }
}
