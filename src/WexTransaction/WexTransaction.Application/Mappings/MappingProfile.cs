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
