using System.Globalization;

namespace WexTransaction.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<PurchaseTransaction, GetPurchaseTransactionResponse>()
            .ConstructUsing((src, ctx) => new GetPurchaseTransactionResponse(
                src.Id,
                src.Description.Value,
                src.TransactionDate,
                src.Amount.ToString("F2", CultureInfo.InvariantCulture),
                "",
                ""
            ));
    }
}
