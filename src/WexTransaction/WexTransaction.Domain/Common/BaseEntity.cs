namespace WexTransaction.Domain.Common;

public abstract class BaseEntity : IEntity
{
    public Guid Id { get; set; }
}