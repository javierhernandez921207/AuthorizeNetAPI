namespace AuthorizeNetAPI.Model
{
    public class UpdateSubscriptionResponse
    {
        public ErrorResponse? Error { get; set; }
        public bool IsUpdated { get; set; } = false;
    }
}
