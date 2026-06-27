namespace WexTransaction.Domain.Common.Interfaces;

public interface IAuditableEntity
{
    public DateTime CreatedAt { get; set; }
}