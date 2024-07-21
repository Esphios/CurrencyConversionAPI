namespace CurrencyConversionService.Models.Dto.In
{
    public class ConversionIn
    {
        public string FromCurrency { get; set; }
        public string ToCurrency { get; set; }
        public decimal Amount { get; set; }
    }
}
