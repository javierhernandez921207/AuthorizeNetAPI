namespace AuthorizeNetAPI.Model
{
    public class ChargeCustomerResponse
    {
        public ErrorResponse? Error { get; set; }
        public string TransactionId { get; set; } = string.Empty;
    }
}
