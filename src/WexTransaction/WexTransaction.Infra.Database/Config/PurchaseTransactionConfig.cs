namespace WexTransaction.Infra.Database.Config;

public sealed class PurchaseTransactionConfig : IEntityTypeConfiguration<PurchaseTransaction>
{
    public void Configure(EntityTypeBuilder<PurchaseTransaction> builder)
    {
        var transactionDescriptionConverter = new ValueConverter<TransactionDescription, string>(
            v => v.Value,
            v => new TransactionDescription(v));

        var moneyConverter = new ValueConverter<Money, decimal>(
            v => v.Value,
            v => new Money(v));

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("transaction_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(p => p.TransactionDate)
            .HasColumnName("transaction_date")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(p => p.Description)
            .HasColumnName("transaction_description")
            .HasColumnType("varchar")
            .HasMaxLength(50)
            .HasConversion(transactionDescriptionConverter)
            .IsRequired();

        builder.Property(p => p.Amount)
            .HasColumnName("transaction_amount")
            .HasPrecision(18,2)
            .HasConversion(moneyConverter)
            .IsRequired();

        builder.ToTable("tb_purchase_transaction");
    }
}