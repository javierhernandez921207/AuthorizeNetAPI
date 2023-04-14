using Microsoft.AspNetCore.Http;
using AuthorizeNet.Api.Controllers;
using AuthorizeNet.Api.Contracts.V1;
using AuthorizeNet.Api.Controllers.Bases;
using Microsoft.AspNetCore.Mvc;
using AuthorizeNetAPI.Model;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Linq;
using AutoMapper;

namespace AuthorizeNetAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public CustomerController(IConfiguration configuration, IMapper mapper)
        {
            _mapper = mapper;
            _configuration = configuration;
            InitConfig();
        }

        private void InitConfig()
        {
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
        }

        [HttpPost]
        public ActionResult CreateCustomer([FromBody] CreateCustomerRequest createCustomerRequest)
        {          
            List<customerPaymentProfileType> paymentProfileList = new();
            if(createCustomerRequest.CreditCardProfiles != null)
                foreach (var ccp in createCustomerRequest.CreditCardProfiles)
                {
                    creditCardType creditCard = _mapper.Map<creditCardType>(ccp.CreditCard);
                    paymentType cc = new () { Item = creditCard };
                    customerPaymentProfileType ccPaymentProfile = new()
                    {
                        payment = cc,
                        billTo = _mapper.Map<customerAddressType>(ccp.BilliTo),
                        customerType = 0
                    };
                    paymentProfileList.Add(ccPaymentProfile);
                }
            if (createCustomerRequest.BankAccountProfiles != null)
                foreach (var bap in createCustomerRequest.BankAccountProfiles)
                {
                    bankAccountType bankAccount = _mapper.Map<bankAccountType>(bap.BankAccount);
                    paymentType echeck = new () { Item = bankAccount };
                    customerPaymentProfileType echeckPaymentProfile = new()
                    {
                        payment = echeck,
                        billTo = _mapper.Map<customerAddressType>(bap.BilliTo),
                        customerType = 0
                    };
                    paymentProfileList.Add(echeckPaymentProfile);
                }

            List<customerAddressType> addressInfoList = new();

            foreach (var address in createCustomerRequest.ShipToList) {
                customerAddressType a = _mapper.Map<customerAddressType>(address);
                addressInfoList.Add(a);
            }

            customerProfileType customerProfile = new()
            {
                merchantCustomerId = createCustomerRequest.CustomerId,
                email = createCustomerRequest.CustomerEmail,
                description = createCustomerRequest.CustomerName,
                paymentProfiles = paymentProfileList.ToArray(),
                shipToList = addressInfoList.ToArray()
            };

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

        [HttpGet("{customerId}")]
        public ActionResult<GetCustomerResponse> GetCustomer(string customerId)
        {
            getCustomerProfileRequest request = new()
            {
                customerProfileId = customerId
            };

            // instantiate the controller that will call the service
            var controller = new getCustomerProfileController(request);
            controller.Execute();

            // get the response from the service (errors contained if any)
            var response = controller.GetApiResponse();

            if (response != null && response.messages.resultCode == messageTypeEnum.Ok)
            {               
                return Ok(new GetCustomerResponse() {
                    Customer = new()
                    {
                        Email = response.profile.email,
                        MerchantCustomerId = response.profile.merchantCustomerId,
                        Description = response.profile.description,
                        CreditCardProfiles = _mapper.Map<List<PaymentCreditCard>>(response.profile.paymentProfiles
                        .Where(response => response.payment.Item.GetType().Equals(typeof(creditCardMaskedType)))
                        .ToList()),
                        BankAccountProfiles = _mapper.Map<List<PaymentBankAccount>>(response.profile.paymentProfiles
                        .Where(response => response.payment.Item.GetType().Equals(typeof(bankAccountMaskedType)))
                        .ToList()),
                        ShipToList = _mapper.Map<List<Address>>(response.profile.shipToList.ToList())                        
                    }
                });
            }
            else if (response != null)
            {                
                return BadRequest(new GetCustomerResponse() { 
                    Error = new() { 
                        Code = response.messages.message[0].code,
                        Message = response.messages.message[0].text 
                    }
                });
            }

            return BadRequest(new GetCustomerResponse()
            {
                Error = new()
                {                    
                    Message = "Customer not found"
                }
            });
        }
    }
}
