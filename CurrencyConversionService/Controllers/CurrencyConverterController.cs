using CurrencyConversionService.Interfaces;
using Microsoft.AspNetCore.Mvc;
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

        [HttpGet("convert")]
        public async Task<IActionResult> Convert(string fromCurrency, string toCurrency, decimal amount)
        {
            var result = await _currencyConverterService.ConvertAsync(fromCurrency, toCurrency, amount);
            return Ok(result);
        }

        [HttpPost("update-rates")]
        public async Task<IActionResult> UpdateRates()
        {
            await _currencyConverterService.UpdateBulkRatesAsync();
            return NoContent();
        }
    }
}
