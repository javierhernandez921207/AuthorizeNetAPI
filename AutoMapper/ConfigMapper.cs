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
        }
    }
}
