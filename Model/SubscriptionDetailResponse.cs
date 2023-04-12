using AuthorizeNetAPI.Enumerations;
using System.Xml.Serialization;

namespace AuthorizeNetAPI.Model
{
    public class SubscriptionDetailResponse
    {
        public string name;
        
        public subscriptionCustomerProfileType profile;

        //public orderType order;
        public int Id { get; set; }
        public string Name { get; set; }
        public SubscriptionStatus Status { get; set; }
        public int TotalOccurrences { get; set; }
        public int PastOccurrences { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public decimal Amount { get; set; }
        public decimal TrialAmount { get; set; }
    }
}
