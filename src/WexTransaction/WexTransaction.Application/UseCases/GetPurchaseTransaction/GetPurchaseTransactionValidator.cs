namespace WexTransaction.Application.UseCases.GetPurchaseTransaction;

public class GetPurchaseTransactionValidator : AbstractValidator<GetPurchaseTransactionRequest>
{
    public GetPurchaseTransactionValidator()
    {   
        RuleFor(g => g.TransactionId)
            .NotNull().WithMessage("Please TransactionId cannot be null.")
            .NotEmpty().WithMessage("Please TransactionId cannot be empty");

        RuleFor(g => g.Country)
            .NotNull().WithMessage("Please Country cannot be null.")
            .NotEmpty().WithMessage("Please Country cannot be empty");

        RuleFor(g => g.Currency)
            .NotNull().WithMessage("Please Currency cannot be null.")
            .NotEmpty().WithMessage("Please Currency cannot be empty");
            
    }
}