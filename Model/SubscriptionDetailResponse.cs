using AuthorizeNetAPI.Enumerations;
using System.Xml.Serialization;

namespace AuthorizeNetAPI.Model
{
    public class SubscriptionDetailResponse
    { 
        public string Name { get; set; } = string.Empty;
        public SubscriptionStatus Status { get; set; }
        public int TotalOccurrences { get; set; }
        public int TrialOccurrences { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public decimal Amount { get; set; }
        public decimal TrialAmount { get; set; }
        public IntervalUnit IntervalUnit { get; set; }
        public short IntervalLength { get; set; }
        public DateTime StartDate { get; set; }
        public string CustomerID { get; set; } = string.Empty;
        public string PaymentID { get; set; } = string.Empty;
        public string ShippingID { get; set; } = string.Empty;
    }
}
