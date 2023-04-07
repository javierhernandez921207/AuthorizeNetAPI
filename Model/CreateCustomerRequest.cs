namespace AuthorizeNetAPI.Model
{
    public class CreateCustomerRequest
    {
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public List<PaymentCreditCard>? CreditCardProfiles { get; set; }
        public List<PaymentBankAccount>? BankAccountProfiles { get; set; }
        public List<Address> ShipToList { get; set; }
    }
}
