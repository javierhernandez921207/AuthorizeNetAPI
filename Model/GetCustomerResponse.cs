namespace AuthorizeNetAPI.Model
{
    public class GetCustomerResponse
    {
        public ErrorResponse Error { get; set; }
        public CustomerResponse Customer { get; set; }
    }

    public class CustomerResponse
    {
        public string Email { get; set; }
        public string MerchantCustomerId { get; set; }
        public string Description { get; set; }
        public List<PaymentCreditCard>? CreditCardProfiles { get; set; }
        public List<PaymentBankAccount>? BankAccountProfiles { get; set; }
        public List<Address>? ShipToList { get; set; }

    }
}
