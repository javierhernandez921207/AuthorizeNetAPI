namespace AuthorizeNetAPI.Model
{
    public class CancelSubscriptionResponse
    {
        public ErrorResponse? Error { get; set; }
        public bool IsCanceled { get; set; } = false;
    }
}
