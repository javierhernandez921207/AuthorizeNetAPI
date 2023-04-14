namespace AuthorizeNetAPI.Model
{
    public class RefundCreditCardRequest
    {
        public decimal Amount { get; set; } = decimal.Zero;
        public string? CardNumber { get; set; }
        public string? CardExpiration { get; set; }
    }
}
