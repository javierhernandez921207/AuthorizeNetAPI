namespace AuthorizeNetAPI.Model
{
    public class GetListSubscriptionResponse
    {
        public ErrorResponse Error { get; set; }
        public List<SubscriptionResponse> Subscriptions { get; set; }
        
    }

}
