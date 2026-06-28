namespace WexTransaction.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<PurchaseTransaction, QueryTransactionResponse>()
            .ForMember(dest => dest.TransactionId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => (string)src.Description))
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.TransactionDate))
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => (decimal)src.Amount))
            .ForMember(dest => dest.TaxRate, opt => opt.Ignore())
            .ForMember(dest => dest.ConvertedValue, opt => opt.Ignore());
    }
}
