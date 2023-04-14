using AuthorizeNet;
using AuthorizeNet.Api.Contracts.V1;
using AuthorizeNetAPI.Model;
using AutoMapper;
using Address = AuthorizeNetAPI.Model.Address;
using BankAccount = AuthorizeNetAPI.Model.BankAccount;

namespace AuthorizeNetAPI.AutoMapper
{
    public class ConfigMapper : Profile
    {
        public ConfigMapper() {
            CreateMap<creditCardMaskedType, CreditCard>();
            CreateMap<bankAccountMaskedType, BankAccount>();

            CreateMap<customerPaymentProfileMaskedType, PaymentCreditCard>()
                .ForMember(des => des.BilliTo, opt => opt.MapFrom(opt => opt.billTo))
                .ForMember(des => des.CreditCard, opt => opt.MapFrom(opt => opt.payment.Item));

            CreateMap<customerPaymentProfileMaskedType, PaymentBankAccount>()
               .ForMember(des => des.BilliTo, opt => opt.MapFrom(opt => opt.billTo))
               .ForMember(des => des.BankAccount, opt => opt.MapFrom(opt => opt.payment.Item));

            CreateMap<CreditCard, creditCardType>();
            CreateMap<creditCardType, CreditCard>();

            CreateMap<creditCardMaskedType, creditCardType>();

            CreateMap<bankAccountType, BankAccount>();
            CreateMap<BankAccount,bankAccountType>();

            CreateMap<customerAddressExType, Address>()
                .ForMember(des => des.Address1, opt => opt.MapFrom(opt => opt.address))
                .ForMember(des => des.ZipCode, opt => opt.MapFrom(opt => opt.zip));
            CreateMap<Address, customerAddressExType>()
                .ForMember(des => des.address, opt => opt.MapFrom(opt => opt.Address1))
                .ForMember(des => des.zip, opt => opt.MapFrom(opt => opt.ZipCode));

            CreateMap<Address, customerAddressType>()
                .ForMember(des => des.address, opt => opt.MapFrom(opt => opt.Address1))
                .ForMember(des => des.zip, opt => opt.MapFrom(opt => opt.ZipCode));
            CreateMap<customerAddressType, Address>()
                .ForMember(des => des.Address1, opt => opt.MapFrom(opt => opt.address))
                .ForMember(des => des.ZipCode, opt => opt.MapFrom(opt => opt.zip));

            CreateMap<SubscriptionDetail, SubscriptionResponse>();
            CreateMap<ARBSubscriptionMaskedType, SubscriptionDetailResponse>()
                .ForMember(des => des.IntervalUnit, opt => opt.MapFrom(opt => opt.paymentSchedule.interval.unit))
                .ForMember(des => des.IntervalLength, opt => opt.MapFrom(opt => opt.paymentSchedule.interval.length))
                .ForMember(des => des.StartDate, opt => opt.MapFrom(opt => opt.paymentSchedule.startDate))
                .ForMember(des => des.TotalOccurrences, opt => opt.MapFrom(opt => opt.paymentSchedule.totalOccurrences))
                .ForMember(des => des.TrialOccurrences, opt => opt.MapFrom(opt => opt.paymentSchedule.trialOccurrences))
                .ForMember(des => des.CustomerID, opt => opt.MapFrom(opt => opt.profile.customerProfileId))
                .ForMember(des => des.PaymentID, opt => opt.MapFrom(opt => opt.profile.paymentProfile.customerPaymentProfileId))
                .ForMember(des => des.ShippingID, opt => opt.MapFrom(opt => opt.profile.shippingProfile.customerAddressId));

            CreateMap<transactionSummaryType, TransactionResponse>();
        }
    }
}
