using AuthorizeNetAPI.Enumerations;

namespace AuthorizeNetAPI.Model
{
    public class SubscriptionResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string CustomerProfileId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public SubscriptionStatus Status { get; set; }
        public int TotalOccurrences { get; set; }
        public int PastOccurrences { get; set; }
    }
}
