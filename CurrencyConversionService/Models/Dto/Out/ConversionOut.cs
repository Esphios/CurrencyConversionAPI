namespace CurrencyConversionService.Models.Dto.Out
{
    public class ConversionOut
    {
        public string FromCurrency { get; set; }
        public string ToCurrency { get; set; }
        public decimal Amount { get; set; }
        public decimal ConvertedAmount { get; set; }
    }
}
