using AuthorizeNetAPI.Enumerations;

namespace AuthorizeNetAPI.Model
{
    public class UpdateSubscriptionRequest
    {
        public string SubscriptionId { get; set; } = string.Empty;
        public string customerProfileId { get; set; } = string.Empty;
        public string customerPaymentProfileId { get; set; } = string.Empty;
        public string customerAddressId { get; set; } = string.Empty;       
        public short TrialOccurrences { get; set; } = -1;
        public short TotalOccurrences { get; set; } = 9999;
        public decimal Amount { get; set; }
        public decimal TrialAmount { get; set; }
    }
}
