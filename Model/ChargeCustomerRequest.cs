namespace AuthorizeNetAPI.Model
{
    public class ChargeCustomerRequest
    {
        public string CustomerProfileId { get; set; } = string.Empty;
        public string CustomerPaymentProfileId { get; set; } = string.Empty;
        public decimal Amount { get; set; } = decimal.Zero;
    }
}
