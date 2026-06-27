namespace WexTransaction.Domain.Common;

public class BaseAuditableEntity : BaseEntity, IAuditableEntity
{
    public DateTime CreatedAt { get; set; }
}
