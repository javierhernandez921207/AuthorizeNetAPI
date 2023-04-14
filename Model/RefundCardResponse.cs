namespace AuthorizeNetAPI.Model
{
    public class RefundCardResponse
    {
        public ErrorResponse? Error { get; set; }
        public string TransactionId { get; set; } = string.Empty;
    }
}
