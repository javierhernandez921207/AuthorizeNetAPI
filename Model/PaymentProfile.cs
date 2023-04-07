using AuthorizeNet.Api.Contracts.V1;

namespace AuthorizeNetAPI.Model
{
    public class PaymentProfile
    {
        public string CustomerType { get; set; }
        public Address BilliTo { get; set; }
    }
    public class CreditCard : PaymentProfile
    {
        public string CardNumber { get; set; }
        public string ExpirationDate { get; set; }
        public string CardCode { get; set; }        
    }
    public class BankAccount : PaymentProfile
    {
        public bankAccountTypeEnum AccountType { get; set; }
        public string RoutingNumber { get; set; }
        public string AccountNumber { get; set; }
        public string NameOnAccount { get; set; }
        public echeckTypeEnum EcheckType { get; set; }
        public string BankName { get; set; }
    }

}
