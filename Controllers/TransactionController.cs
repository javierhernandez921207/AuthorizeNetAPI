using AuthorizeNet.Api.Contracts.V1;
using AuthorizeNet.Api.Controllers;
using AuthorizeNet.Api.Controllers.Bases;
using AuthorizeNetAPI.Model;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthorizeNetAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        public TransactionController(IConfiguration configuration, IMapper mapper)
        {
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

        [HttpGet("List")]
        public ActionResult GetTRansactionList()
        {            
            string batchId = "6680535";

            var request = new getTransactionListRequest();
            request.batchId = batchId;
            request.paging = new Paging
            {
                limit = 5,
                offset = 1
            };
            request.sorting = new TransactionListSorting
            {
                orderBy = TransactionListOrderFieldEnum.id,
                orderDescending = true
            };

            // instantiate the controller that will call the service
            var controller = new getTransactionListController(request);
            controller.Execute();

            // get the response from the service (errors contained if any)
            var response = controller.GetApiResponse();

            if (response != null && response.messages.resultCode == messageTypeEnum.Ok)
            {
                List<TransactionResponse> transactions = new();
                if (response.transactions == null)
                    return Ok(new GetTransactionCustomerResponse());

                foreach (var t in response.transactions)
                {
                    transactions.Add(_mapper.Map<TransactionResponse>(t));
                }
                return Ok(new GetTransactionCustomerResponse() { Transactions = transactions });
            }
            else if (response != null)
            {
                return BadRequest(new GetTransactionCustomerResponse()
                {
                    Error = new()
                    {
                        Code = response.messages.message[0].code,
                        Message = response.messages.message[0].text
                    }
                });
            }

            return BadRequest(new GetTransactionCustomerResponse()
            {
                Error = new()
                {
                    Message = "Internal Server Error"
                }
            });
        }

        [HttpGet("customer/{idCustomer}")]
        public ActionResult<GetTransactionCustomerResponse> GetTransactionCustomer([FromRoute] string idCustomer)
        {
            var request = new getTransactionListForCustomerRequest
            {
                customerProfileId = idCustomer
            };

            var controller = new getTransactionListForCustomerController(request);
            controller.Execute();

            var response = controller.GetApiResponse();

            if (response != null && response.messages.resultCode == messageTypeEnum.Ok)
            {
                List<TransactionResponse> transactions = new ();
                if (response.transactions == null)
                    return Ok(new GetTransactionCustomerResponse());

                foreach (var t in response.transactions)
                {
                    transactions.Add(_mapper.Map<TransactionResponse>(t));
                }
                return Ok(new GetTransactionCustomerResponse() { Transactions = transactions });
            }
            else if (response != null)
            {
                return BadRequest(new GetTransactionCustomerResponse()
                {
                    Error = new()
                    {
                        Code = response.messages.message[0].code,
                        Message = response.messages.message[0].text
                    }
                });
            }

            return BadRequest(new GetTransactionCustomerResponse()
            {
                Error = new()
                {                    
                    Message = "Internal Server Error"
                }
            });
        }

        [HttpPost("charge_customer")]
        public ActionResult<ChargeCustomerResponse> ChargeCustomer([FromBody]ChargeCustomerRequest request)
        {
            //create a customer payment profile
            customerProfilePaymentType profileToCharge = new()
            {
                customerProfileId = request.CustomerProfileId,
                paymentProfile = new paymentProfile { paymentProfileId = request.CustomerPaymentProfileId }
            };

            var transactionRequest = new transactionRequestType
            {
                transactionType = transactionTypeEnum.authCaptureTransaction.ToString(),
                amount = request.Amount,
                profile = profileToCharge
            };

            var requestTransaction = new createTransactionRequest { transactionRequest = transactionRequest };

            var controller = new createTransactionController(requestTransaction);
            controller.Execute();
            
            var response = controller.GetApiResponse();

            if (response != null)
            {
                if (response.messages.resultCode == messageTypeEnum.Ok)
                {
                    if (response.transactionResponse.messages != null)
                    {
                        return Ok(new ChargeCustomerResponse() 
                        {
                            TransactionId = response.transactionResponse.transId
                        });
                    }
                    else
                    {                        
                        if (response.transactionResponse.errors != null)
                        {                            
                            return BadRequest(new ChargeCustomerResponse()
                            {
                                Error = new ErrorResponse()
                                {
                                    Code = response.transactionResponse.errors[0].errorCode,
                                    Message = response.transactionResponse.errors[0].errorText
                                }
                            });
                        }
                        else
                        {
                            return BadRequest(new ChargeCustomerResponse()
                            {
                                Error = new ErrorResponse()
                                {
                                    Message = "Internal Server Error"
                                }
                            });
                        }
                    }
                }
                else
                {                    
                    if (response.transactionResponse != null && response.transactionResponse.errors != null)
                    {
                        return BadRequest(new ChargeCustomerResponse()
                        {
                            Error = new ErrorResponse()
                            {
                                Code = response.transactionResponse.errors[0].errorCode,
                                Message = response.transactionResponse.errors[0].errorText
                            }
                        });
                    }
                    else
                    {                        
                        return BadRequest(new ChargeCustomerResponse()
                        {
                            Error = new ErrorResponse()
                            {
                                Code = response.messages.message[0].code,
                                Message = response.messages.message[0].text
                            }
                        });
                    }
                }
            }
            else
            {
                return BadRequest(new ChargeCustomerResponse()
                {
                    Error = new ErrorResponse()
                    {
                        Message = "Internal Server Error"
                    }
                });
            }
        }

        [HttpPost("refund-transaction")]
        public ActionResult<RefundCardResponse> RefundTransaction([FromBody] RefundTransactionRequest request)
        {
            //Get Transaction Detail
            var requestTansaction = new getTransactionDetailsRequest
            {
                transId = request.TransactionId
            };

            // instantiate the controller that will call the service
            var controller = new getTransactionDetailsController(requestTansaction);
            controller.Execute();

            // get the response from the service (errors contained if any)
            var response = controller.GetApiResponse();

            creditCardType creditCard = new creditCardType();

            if (response != null && response.messages.resultCode == messageTypeEnum.Ok)
            {
                if (response.transaction == null)
                    return BadRequest(new ChargeCustomerResponse()
                    {
                        Error = new ErrorResponse()
                        {
                            Message = "Transaction not found"
                        }
                    });

                creditCard = _mapper.Map<creditCardType>(response.transaction.payment.Item);
            }

            if (creditCard.cardNumber == null)
                return BadRequest(new ChargeCustomerResponse()
                {
                    Error = new ErrorResponse()
                    {
                        Message = "Transaction not found"
                    }
                });
           
            var paymentType = new paymentType { Item = creditCard };

            var transactionRequest = new transactionRequestType
            {
                transactionType = transactionTypeEnum.refundTransaction.ToString(),
                payment = paymentType,
                amount = request.Amount,
                refTransId = request.TransactionId
            };

            var requestRefund = new createTransactionRequest { transactionRequest = transactionRequest };
            
            var controller2 = new createTransactionController(requestRefund);
            controller2.Execute();

            var response2 = controller2.GetApiResponse();

            if (response2 != null)
            {
                if (response2.messages.resultCode == messageTypeEnum.Ok)
                {
                    if (response2.transactionResponse.messages != null)
                    {
                        return Ok( new RefundCardResponse() { TransactionId = response2.transactionResponse.transId});
                    }
                    else
                    {                       
                        if (response2.transactionResponse.errors != null)
                        {
                            return BadRequest(new ChargeCustomerResponse()
                            {
                                Error = new ErrorResponse()
                                {
                                    Code = response2.transactionResponse.errors[0].errorCode,
                                    Message = response2.transactionResponse.errors[0].errorText
                                }
                            });
                        }
                        else
                        {
                            return BadRequest(new ChargeCustomerResponse()
                            {
                                Error = new ErrorResponse()
                                {
                                    Message = "Internal Server Error"
                                }
                            });
                        }
                    }
                }
                else
                {
                    if (response2.transactionResponse != null && response2.transactionResponse.errors != null)
                    {
                        return BadRequest(new ChargeCustomerResponse()
                        {
                            Error = new ErrorResponse()
                            {
                                Code = response2.transactionResponse.errors[0].errorCode,
                                Message = response2.transactionResponse.errors[0].errorText
                            }
                        });
                    }
                    else
                    {
                        return BadRequest(new ChargeCustomerResponse()
                        {
                            Error = new ErrorResponse()
                            {
                                Code = response2.messages.message[0].code,
                                Message = response2.messages.message[0].text
                            }
                        });
                    }
                }
            }
            else
            {
                return BadRequest(new ChargeCustomerResponse()
                {
                    Error = new ErrorResponse()
                    {
                        Message = "Internal Server Error"
                    }
                });
            }            
        }

        [HttpPost("refund-credit-card")]
        public ActionResult<RefundCardResponse> RefundCreditCard([FromBody] RefundCreditCardRequest request)
        {
            creditCardType creditCard = new()
            {
                cardNumber = request.CardNumber,
                expirationDate = request.CardExpiration
            };

            var paymentType = new paymentType { Item = creditCard };

            var transactionRequest = new transactionRequestType
            {
                transactionType = transactionTypeEnum.refundTransaction.ToString(),
                payment = paymentType,
                amount = request.Amount
            };

            var requestRefund = new createTransactionRequest { transactionRequest = transactionRequest };

            var controller2 = new createTransactionController(requestRefund);
            controller2.Execute();

            var response2 = controller2.GetApiResponse();

            if (response2 != null)
            {
                if (response2.messages.resultCode == messageTypeEnum.Ok)
                {
                    if (response2.transactionResponse.messages != null)
                    {
                        return Ok(new RefundCardResponse() { TransactionId = response2.transactionResponse.transId });
                    }
                    else
                    {
                        if (response2.transactionResponse.errors != null)
                        {
                            return BadRequest(new ChargeCustomerResponse()
                            {
                                Error = new ErrorResponse()
                                {
                                    Code = response2.transactionResponse.errors[0].errorCode,
                                    Message = response2.transactionResponse.errors[0].errorText
                                }
                            });
                        }
                        else
                        {
                            return BadRequest(new ChargeCustomerResponse()
                            {
                                Error = new ErrorResponse()
                                {
                                    Message = "Internal Server Error"
                                }
                            });
                        }
                    }
                }
                else
                {
                    if (response2.transactionResponse != null && response2.transactionResponse.errors != null)
                    {
                        return BadRequest(new ChargeCustomerResponse()
                        {
                            Error = new ErrorResponse()
                            {
                                Code = response2.transactionResponse.errors[0].errorCode,
                                Message = response2.transactionResponse.errors[0].errorText
                            }
                        });
                    }
                    else
                    {
                        return BadRequest(new ChargeCustomerResponse()
                        {
                            Error = new ErrorResponse()
                            {
                                Code = response2.messages.message[0].code,
                                Message = response2.messages.message[0].text
                            }
                        });
                    }
                }
            }
            else
            {
                return BadRequest(new ChargeCustomerResponse()
                {
                    Error = new ErrorResponse()
                    {
                        Message = "Internal Server Error"
                    }
                });
            }
        }
    }
}
