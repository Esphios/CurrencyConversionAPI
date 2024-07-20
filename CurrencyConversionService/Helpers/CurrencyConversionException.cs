using System;

namespace CurrencyConversionService.Helpers
{
    public class CurrencyConversionException : Exception
    {
        public CurrencyConversionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public CurrencyConversionException(string message)
            : base(message)
        {
        }
    }
}
