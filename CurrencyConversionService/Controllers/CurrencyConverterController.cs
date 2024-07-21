using CurrencyConversionService.Interfaces;
using CurrencyConversionService.Models.Dto.In;
using CurrencyConversionService.Models.Dto.Out;
using CurrencyConversionService.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
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
        /// Converts currencies from one to another.
        /// </summary>
        /// <param name="conversionRequests">List of conversion requests.</param>
        /// <returns>List of conversion results.</returns>
        /// <response code="200">Returns the list of converted amounts.</response>
        /// <response code="400">If any currency code is invalid.</response>
        [HttpPost("convert")]
        public async Task<IActionResult> BulkConvert([FromBody] List<ConversionIn> conversionRequests)
        {
            var invalidCurrencies = conversionRequests
                .SelectMany(r => new[] { r.FromCurrency, r.ToCurrency })
                .Distinct()
                .Where(c => !Enum.TryParse<Currency>(c, true, out var _))
                .ToList();

            if (invalidCurrencies.Any())
            {
                return BadRequest($"Invalid currencies: {string.Join(", ", invalidCurrencies)}. Please use valid currency codes.");
            }

            var results = new List<ConversionOut>();

            foreach (var request in conversionRequests)
            {
                var convertedAmount = await _currencyConverterService.ConvertAsync(request.FromCurrency, request.ToCurrency, request.Amount);
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
            await _currencyConverterService.UpdateBulkRatesAsync();
            return NoContent();
        }
    }
}
