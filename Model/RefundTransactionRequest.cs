namespace AuthorizeNetAPI.Model
{
    public class RefundTransactionRequest
    {
        public string TransactionId { get; set; } = string.Empty;
        public decimal Amount { get; set; } = decimal.Zero;
    }
}
