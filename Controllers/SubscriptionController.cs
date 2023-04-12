using AuthorizeNet.Api.Contracts.V1;
using AuthorizeNet.Api.Controllers;
using AuthorizeNet.Api.Controllers.Bases;
using AuthorizeNetAPI.Enumerations;
using AuthorizeNetAPI.Model;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthorizeNetAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        public SubscriptionController(IConfiguration configuration, IMapper mapper) {
            _configuration = configuration;
            _mapper = mapper;
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
        public ActionResult<CreateSubscriptionResponse> CreateSubscription([FromBody] CreateSubscriptionRequest request)
        {
            if (request == null)
            {
                return BadRequest(new CreateSubscriptionResponse()
                {
                    Error = new ErrorResponse()
                    {
                        Message = "Incorret request"
                    }
                });
            }

            paymentScheduleTypeInterval interval = new()
            {
                length = request.SubscriptionRequest.IntervalLength,
                unit = (ARBSubscriptionUnitEnum)request.SubscriptionRequest.IntervalUnit
            };

            paymentScheduleType schedule = new()
            {
                interval = interval,
                startDate = request.SubscriptionRequest.StartDate,
                totalOccurrences = request.SubscriptionRequest.TotalOccurrences,
                trialOccurrences = request.SubscriptionRequest.TrialOccurrences
            };

            customerProfileIdType customerProfile = new()
            {
                customerProfileId = request.customerProfileId,
                customerPaymentProfileId = request.customerPaymentProfileId,
                customerAddressId = request.customerAddressId
            };

            ARBSubscriptionType subscriptionType = new()
            {
                amount = request.SubscriptionRequest.Amount,
                trialAmount = request.SubscriptionRequest.TrialAmount,
                paymentSchedule = schedule,
                profile = customerProfile
            };

            var createSubscRequest = new ARBCreateSubscriptionRequest { subscription = subscriptionType };

            var controller = new ARBCreateSubscriptionController(createSubscRequest);
            controller.Execute();

            ARBCreateSubscriptionResponse response = controller.GetApiResponse();

            if (response != null && response.messages.resultCode == messageTypeEnum.Ok)
            {
                if (response != null && response.messages.message != null)
                {
                    return Ok(new CreateSubscriptionResponse()
                    {
                        SubscriptionId = response.subscriptionId
                    });
                }
            }
            else if (response != null)
            {
                return BadRequest(
                    new CreateSubscriptionResponse()
                    {
                        Error = new()
                        {
                            Code = response.messages.message[0].code,
                            Message = response.messages.message[0].text
                        }
                    });
            }

            return BadRequest(new CreateSubscriptionResponse()
            {
                Error = new() { Message = "Internal error" }
            });
        }

        [HttpGet("list_type/{type}")]
        public ActionResult<GetListSubscriptionResponse> GetListSubscription([FromRoute]SubscriptionSearchType type) {

            var request = new ARBGetSubscriptionListRequest { searchType = (ARBGetSubscriptionListSearchTypeEnum)type };
            var controller = new ARBGetSubscriptionListController(request);
            controller.Execute();

            ARBGetSubscriptionListResponse response = controller.GetApiResponse();
            
            if (response != null && response.messages.resultCode == messageTypeEnum.Ok)
            {
                if (response != null && response.messages.message != null && response.subscriptionDetails != null)
                {
                    return Ok(new GetListSubscriptionResponse() { 
                        Subscriptions = _mapper.Map<List<SubscriptionResponse>>(response.subscriptionDetails)
                    });
                }
            }
            else if (response != null)
            {
                return BadRequest(new GetListSubscriptionResponse()
                {
                    Error = new ErrorResponse() { 
                        Code = response.messages.message[0].code,
                        Message = response.messages.message[0].text
                    }
                });
            }

            return BadRequest(new GetListSubscriptionResponse()
            {
                Error = new ErrorResponse()
                {                    
                    Message = "Internal Server Error"
                }
            });
        }

        [HttpGet("{Id}")]
        public ActionResult<GetSubscriptionResponse> GetSubscription([FromRoute] string Id) {
            if (string.IsNullOrEmpty(Id))
                return BadRequest(new GetListSubscriptionResponse()
                {
                    Error = new ErrorResponse()
                    {
                        Message = "Subscription Id not found"
                    }
                });

            var request = new ARBGetSubscriptionRequest { subscriptionId = Id };

            var controller = new ARBGetSubscriptionController(request);
            controller.Execute();

            ARBGetSubscriptionResponse response = controller.GetApiResponse();

            if (response != null && response.messages.resultCode == messageTypeEnum.Ok)
            {
                if (response.subscription != null)
                {                    
                    return Ok(new GetSubscriptionResponse() { 
                        SubscriptionDetail = _mapper.Map<SubscriptionDetailResponse>(response.subscription)
                    });;
                }
            }
            else if (response != null)
            {
                if (response.messages.message.Length > 0)
                {                    
                    return BadRequest(new GetSubscriptionResponse() { 
                        Error = new ErrorResponse()
                        {
                            Code = response.messages.message[0].code,
                            Message = response.messages.message[0].text
                        }
                    });
                }
            }
            else
            {
                if (controller.GetErrorResponse().messages.message.Length > 0)
                {
                    return BadRequest(new GetSubscriptionResponse()
                    {
                        Error = new ErrorResponse()
                        {
                            Code = controller.GetErrorResponse().messages.message[0].code,
                            Message = controller.GetErrorResponse().messages.message[0].text
                        }
                    });
                }
            }

            return BadRequest(new GetListSubscriptionResponse()
            {
                Error = new ErrorResponse()
                {
                    Message = "Internal Server Error"
                }
            });
        }

        [HttpPost("cancel/{Id}")]
        public ActionResult<CancelSubscriptionResponse> CancelSubscription([FromRoute] string Id)
        {
            //Please change the subscriptionId according to your request
            var request = new ARBCancelSubscriptionRequest { subscriptionId = Id };
            var controller = new ARBCancelSubscriptionController(request); 
            controller.Execute();

            ARBCancelSubscriptionResponse response = controller.GetApiResponse();

            // validate response
            if (response != null && response.messages.resultCode == messageTypeEnum.Ok)
            {
                if (response != null && response.messages.message != null)
                {
                    return Ok(new CancelSubscriptionResponse() { IsCanceled = true});
                }
            }
            else if (response != null)
            {
                return BadRequest(new CancelSubscriptionResponse() { 
                    Error = new ErrorResponse() {
                        Code = response.messages.message[0].code,
                        Message = response.messages.message[0].text
                    } });
            }

            return BadRequest(new CancelSubscriptionResponse() { 
                Error = new ErrorResponse() { 
                    Message = "Internal Server Error" 
                } }); ;
        }

        [HttpPut]
        public ActionResult UpdateSubscription([FromBody] UpdateSubscriptionRequest request)
        {
            if (request == null)
            {
                return BadRequest(new CreateSubscriptionResponse()
                {
                    Error = new ErrorResponse()
                    {
                        Message = "Incorret request"
                    }
                });
            }
            bool started = true;
            //Search Subscription
            var subscription = new ARBGetSubscriptionRequest { subscriptionId = request.SubscriptionId };
            var controller = new ARBGetSubscriptionController(subscription);
            controller.Execute();
            ARBGetSubscriptionResponse response = controller.GetApiResponse();
            if (response != null && response.messages.resultCode == messageTypeEnum.Ok)
            {
                if (response.subscription != null)
                {
                    if(response.subscription.paymentSchedule.startDate > DateTime.Today)
                    {
                        started = false;
                    }
                }
            }
            paymentScheduleType? schedule = null;
            if (!started)
            {
                schedule = new()
                {
                    totalOccurrences = request.TotalOccurrences,
                    trialOccurrences = request.TrialOccurrences
                };
            }
            

            customerProfileIdType customerProfile = new()
            { 
                customerProfileId = request.customerProfileId,
                customerPaymentProfileId = request.customerPaymentProfileId,
                customerAddressId = request.customerAddressId
            };

            ARBSubscriptionType subscriptionType;
            if (started)
                subscriptionType = new()
                {
                    amount = request.Amount,
                    trialAmount = request.TrialAmount,
                    profile = customerProfile
                };
            else
                subscriptionType = new()
                {
                    amount = request.Amount,
                    trialAmount = request.TrialAmount,
                    paymentSchedule = schedule,
                    profile = customerProfile
                };

            var updateRequest = new ARBUpdateSubscriptionRequest { subscription = subscriptionType, subscriptionId = request.SubscriptionId };
            var controller2 = new ARBUpdateSubscriptionController(updateRequest);
            controller2.Execute();

            ARBUpdateSubscriptionResponse response2 = controller2.GetApiResponse();

            if (response2 != null && response2.messages.resultCode == messageTypeEnum.Ok)
            {
                if (response2 != null && response2.messages.message != null)
                {
                    return Ok(new UpdateSubscriptionResponse() { IsUpdated = true});
                }
            }
            else if (response2 != null)
            {
                return Ok(new UpdateSubscriptionResponse()
                {
                    Error = new ErrorResponse()
                    {
                        Code = response2.messages.message[0].code,
                        Message = response2.messages.message[0].text
                    }
                });
            }

            return Ok(new UpdateSubscriptionResponse()
            {
                Error = new ErrorResponse()
                {
                    Message = "Internal Server Error"
                }
            });

        }
    }
}
