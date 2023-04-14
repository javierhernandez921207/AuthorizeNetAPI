namespace AuthorizeNetAPI.Model
{
    public class GetTransactionCustomerResponse
    {
        public ErrorResponse? Error { get; set; }
        public List<TransactionResponse> Transactions { get; set; } = new ();
    }
}
