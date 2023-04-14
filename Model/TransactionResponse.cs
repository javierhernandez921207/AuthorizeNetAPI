namespace AuthorizeNetAPI.Model
{
    public class TransactionResponse
    {
        public string TransId { get; set; } = string.Empty;
        public DateTime? SubmitTimeLocal { get; set; }
        public string TransactionStatus { get; set; } = string.Empty;
        public decimal SettleAmount { get; set; }

    }
}
