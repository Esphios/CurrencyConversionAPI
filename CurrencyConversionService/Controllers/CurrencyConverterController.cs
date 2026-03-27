using CurrencyConversionService.Interfaces;
using CurrencyConversionService.Models.Dto.In;
using CurrencyConversionService.Models.Dto.Out;
using CurrencyConversionService.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CurrencyConversionService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CurrencyConverterController(ICurrencyConverterService currencyConverterService) : ControllerBase
{

    /// <summary>
    /// Converts currencies from one to another.
    /// </summary>
    /// <param name="conversionRequests">List of conversion requests.</param>
    /// <returns>List of conversion results.</returns>
    /// <response code="200">Returns the list of converted amounts.</response>
    /// <response code="400">If any currency code is invalid.</response>
    [HttpPost("convert")]
    public async Task<IActionResult> BulkConvert([FromBody] List<ConversionIn> conversionRequests)
    {
        List<string> invalidCurrencies = [];

        foreach (ConversionIn request in conversionRequests)
        {
            if (!Enum.TryParse<Currency>(request.FromCurrency, true, out _)
                && !invalidCurrencies.Contains(request.FromCurrency, StringComparer.OrdinalIgnoreCase))
            {
                invalidCurrencies.Add(request.FromCurrency);
            }

            if (!Enum.TryParse<Currency>(request.ToCurrency, true, out _)
                && !invalidCurrencies.Contains(request.ToCurrency, StringComparer.OrdinalIgnoreCase))
            {
                invalidCurrencies.Add(request.ToCurrency);
            }
        }

        if (invalidCurrencies.Count > 0)
        {
            return BadRequest($"Invalid currencies: {string.Join(", ", invalidCurrencies)}. Please use valid currency codes.");
        }

        List<ConversionOut> results = [];

        foreach (ConversionIn request in conversionRequests)
        {
            decimal convertedAmount = await currencyConverterService.ConvertAsync(request.FromCurrency, request.ToCurrency, request.Amount);
            results.Add(new ConversionOut
            {
                FromCurrency = request.FromCurrency,
                ToCurrency = request.ToCurrency,
                Amount = request.Amount,
                ConvertedAmount = convertedAmount
            });
        }

        return Ok(results);
    }

    /// <summary>
    /// Triggers an update of currency rates from an external source.
    /// </summary>
    /// <remarks>
    /// This endpoint is used to refresh the currency conversion rates by fetching the latest data from the configured external source.
    /// The update operation is performed asynchronously, and the endpoint will return a `204 No Content` response upon successful completion.
    /// </remarks>
    /// <response code="204">Indicates that the currency rates were successfully updated.</response>
    /// <response code="500">If there is an internal server error while attempting to update the rates.</response>
    [HttpPost("update-rates")]
    public async Task<IActionResult> UpdateRates()
    {
        await currencyConverterService.UpdateBulkRatesAsync();
        return NoContent();
    }
}
