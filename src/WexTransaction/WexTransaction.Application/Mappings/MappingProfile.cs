namespace WexTransaction.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<PurchaseTransaction, GetPurchaseTransactionResponse>()
            .ConstructUsing((src, ctx) => new GetPurchaseTransactionResponse(
                src.Id,
                (string)src.Description,
                src.TransactionDate,
                (decimal)src.Amount,
                0m,
                0m
            ));
    }
}
