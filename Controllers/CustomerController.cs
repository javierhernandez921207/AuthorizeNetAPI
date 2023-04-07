using Microsoft.AspNetCore.Http;
using AuthorizeNet.Api.Controllers;
using AuthorizeNet.Api.Contracts.V1;
using AuthorizeNet.Api.Controllers.Bases;
using Microsoft.AspNetCore.Mvc;
using AuthorizeNetAPI.Model;

namespace AuthorizeNetAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public CustomerController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<ActionResult> CreateCustomer([FromBody] CreateCustomerRequest createCustomerRequest)
        {            
            Console.WriteLine("Create Customer Profile Sample");

            // set whether to use the sandbox environment, or production enviornment
            bool isProd = _configuration.GetSection("AuthorizeNet").GetSection("isProduction").Value == "true";
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = isProd ? AuthorizeNet.Environment.PRODUCTION : AuthorizeNet.Environment.SANDBOX;

            // define the merchant information (authentication / transaction id)
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
            {
                name = _configuration.GetSection("AuthorizeNet").GetSection("ApiLoginID").Value,
                ItemElementName = ItemChoiceType.transactionKey,
                Item = _configuration.GetSection("AuthorizeNet").GetSection("ApiTransactionKey").Value,
            };

            List<customerPaymentProfileType> paymentProfileList = new List<customerPaymentProfileType>();
            foreach (var ccp in createCustomerRequest.CreditCardProfiles)
            {               
                var creditCard = new creditCardType
                {
                    cardNumber = ccp.CardNumber,
                    expirationDate = ccp.ExpirationDate,
                    cardCode = ccp.CardCode
                };
                paymentType cc = new paymentType { Item = creditCard };
                customerPaymentProfileType ccPaymentProfile = new()
                {
                    payment = cc,
                    billTo = new customerAddressType() 
                    {
                        address = ccp.BilliTo.Address1,
                        firstName = ccp.BilliTo.FirstName,
                        lastName = ccp.BilliTo.LastName,
                        city = ccp.BilliTo.City,
                        country = ccp.BilliTo.Country,  
                        state = ccp.BilliTo.State,
                        zip = ccp.BilliTo.ZipCode,
                        phoneNumber = ccp.BilliTo.PhoneNumber,
                    },
                    customerType = 0
                };
                paymentProfileList.Add(ccPaymentProfile);
            }
            foreach (var bap in createCustomerRequest.BankAccountProfiles)
            {                
                var bankAccount = new bankAccountType
                {
                    accountNumber = bap.AccountNumber,
                    routingNumber = bap.RoutingNumber,
                    accountType = bankAccountTypeEnum.checking,
                    echeckType = echeckTypeEnum.WEB,
                    nameOnAccount = bap.NameOnAccount,
                    bankName = bap.BankName
                };
                paymentType echeck = new paymentType { Item = bankAccount };
                customerPaymentProfileType echeckPaymentProfile = new()
                {
                    payment = echeck,
                    billTo = new customerAddressType()
                    {
                        address = bap.BilliTo.Address1,
                        firstName = bap.BilliTo.FirstName,
                        lastName = bap.BilliTo.LastName,
                        city = bap.BilliTo.City,
                        country = bap.BilliTo.Country,
                        state = bap.BilliTo.State,
                        zip = bap.BilliTo.ZipCode,
                        phoneNumber = bap.BilliTo.PhoneNumber,
                    },
                    customerType = 0
                };
                paymentProfileList.Add(echeckPaymentProfile);
            }

            List<customerAddressType> addressInfoList = new List<customerAddressType>();

            foreach (var address in createCustomerRequest.ShipToList) {
                customerAddressType a = new()
                {
                    firstName = address.FirstName,
                    lastName = address.LastName,
                    address = address.Address1,
                    city = address.City,
                    zip = address.ZipCode,
                    country = address.Country,
                    state = address.State,
                    phoneNumber = address.PhoneNumber,                    
                };
                addressInfoList.Add(a);
            }
            
            customerProfileType customerProfile = new customerProfileType();
            customerProfile.merchantCustomerId = createCustomerRequest.CustomerId;
            customerProfile.email = createCustomerRequest.CustomerEmail;
            customerProfile.description = createCustomerRequest.CustomerName;
            customerProfile.paymentProfiles = paymentProfileList.ToArray();
            customerProfile.shipToList = addressInfoList.ToArray();

            var request = new createCustomerProfileRequest { profile = customerProfile, validationMode = validationModeEnum.none };

            // instantiate the controller that will call the service
            var controller = new createCustomerProfileController(request);
            controller.Execute();

            // get the response from the service (errors contained if any)
            createCustomerProfileResponse response = controller.GetApiResponse();

            // validate response 
            if (response != null)
            {
                if (response.messages.resultCode == messageTypeEnum.Ok)
                {
                    if (response.messages.message != null)
                    {
                        return Ok(new CreateCustomerResponse() { 
                            CustomerProfileId = response.customerProfileId,
                            CustomerPaymentProfileIdList = response.customerPaymentProfileIdList[0],
                            CustomerShippingAddressIdList = response.customerShippingAddressIdList[0]
                        });
                    }
                    return BadRequest(response);
                }
                else
                {                    
                    return BadRequest(new ErrorResponse() { 
                        Code = response.messages.message[0].code,
                        Message = response.messages.message[0].text
                    });
                }
            }
            else
            {
                if (controller.GetErrorResponse().messages.message.Length > 0)
                {
                    return BadRequest(new ErrorResponse()
                    {
                        Code = controller.GetErrorResponse().messages.message[0].code,
                        Message = controller.GetErrorResponse().messages.message[0].text
                    });
                }
                else
                {
                    return BadRequest(new ErrorResponse()
                    {
                        Code = "unknow",
                        Message = "null response from Authorize"
                    });
                }
            }
            
        }
    }
}
