namespace AuthorizeNetAPI.Model
{
    public class CreateSubscriptionRequest
    {
        public string customerProfileId { get; set; }
        public string customerPaymentProfileId { get; set; }
        public string customerAddressId { get; set; }
        public SubscriptionRequest SubscriptionRequest { get; set; }

    }
}
