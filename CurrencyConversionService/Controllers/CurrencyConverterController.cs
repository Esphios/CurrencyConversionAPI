using CurrencyConversionService.Interfaces;
using CurrencyConversionService.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CurrencyConversionService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CurrencyConverterController : ControllerBase
    {
        private readonly ICurrencyConverterService _currencyConverterService;

        public CurrencyConverterController(ICurrencyConverterService currencyConverterService)
        {
            _currencyConverterService = currencyConverterService;
        }

        /// <summary>
        /// Converts an amount from one currency to another.
        /// </summary>
        /// <param name="fromCurrency">The currency code to convert from (e.g., USD, EUR).</param>
        /// <param name="toCurrency">The currency code to convert to (e.g., USD, EUR).</param>
        /// <param name="amount">The amount of money to convert.</param>
        /// <returns>The converted amount.</returns>
        /// <response code="200">Returns the converted amount.</response>
        /// <response code="400">If any currency code is invalid.</response>
        [HttpGet("convert")]
        public async Task<IActionResult> Convert(
            [FromQuery] string fromCurrency,
            [FromQuery] string toCurrency,
            [FromQuery] decimal amount)
        {
            if (!Enum.TryParse<Currency>(fromCurrency, true, out var _))
            {
                return BadRequest($"Invalid fromCurrency: {fromCurrency}. Please use one of the following: {string.Join(", ", Enum.GetNames(typeof(Currency)))}");
            }

            if (!Enum.TryParse<Currency>(toCurrency, true, out var _))
            {
                return BadRequest($"Invalid toCurrency: {toCurrency}. Please use one of the following: {string.Join(", ", Enum.GetNames(typeof(Currency)))}");
            }

            var result = await _currencyConverterService.ConvertAsync(fromCurrency, toCurrency, amount);
            return Ok(result.ToString("F2"));
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
            await _currencyConverterService.UpdateBulkRatesAsync();
            return NoContent();
        }
    }
}
