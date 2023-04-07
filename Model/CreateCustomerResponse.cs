namespace AuthorizeNetAPI.Model
{
    public class CreateCustomerResponse
    {
        public string CustomerProfileId { get; set; }
        public string CustomerPaymentProfileIdList { get; set; }
        public string CustomerShippingAddressIdList { get; set; }
    }
}
