namespace WexTransaction.Application.UseCases.SavePurchaseTransaction;

public class SaveTransactionValidator : AbstractValidator<SaveTransactionCommand>
{
    public SaveTransactionValidator()
    {
        RuleFor(p => p.Description)
            .NotEmpty().WithMessage("Please ensure you have entered the Description")
            .Length(1, 50).WithMessage("The Description must have between 1 and 50 characteres.");
            
        RuleFor(p => p.Date)
            .NotEmpty().WithMessage("Please ensure you have entered a Date");

        RuleFor(p => p.Amount)
            .GreaterThan(0).WithMessage("Please ensure you have entered a valid Amount greater than zero");
    }
}