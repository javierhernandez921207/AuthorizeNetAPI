using AuthorizeNetAPI.Enumerations;

namespace AuthorizeNetAPI.Model
{
    public class SubscriptionRequest
    {
        public IntervalUnit IntervalUnit { get; set; }
        public short IntervalLength { get; set; }
        public DateTime StartDate { get; set; }
        public short TrialOccurrences { get; set; }
        public short TotalOccurrences { get; set; } = 9999;
        public decimal Amount { get; set; }
        public decimal TrialAmount { get; set; }

    }
}
