using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConversionService.Interfaces
{
    public interface ICurrencyConverterService
    {
        Task<decimal> ConvertAsync(string fromCurrency, string toCurrency, decimal amount);
        Task UpdateBulkRatesAsync();
    }
}
