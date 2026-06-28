namespace WexTransaction.Application.Mappings;

using AutoMapper;
using WexTransaction.Application.UseCases.GetPurchaseTransaction;
using WexTransaction.Application.UseCases.SavePurchaseTransaction;
using WexTransaction.Domain.Entities;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<PurchaseTransaction, GetPurchaseTransactionResponse>()
            .ForMember(dest => dest.TransactionId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => (string)src.Description))
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.TransactionDate))
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => (decimal)src.Amount))
            .ForMember(dest => dest.TaxRate, opt => opt.Ignore())
            .ForMember(dest => dest.ConvertedValue, opt => opt.Ignore());
    }
}
