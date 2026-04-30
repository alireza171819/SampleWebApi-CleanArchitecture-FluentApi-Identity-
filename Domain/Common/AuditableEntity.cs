namespace Domain.Common;

public abstract class AuditableEntity : BaseEntity
{
    protected AuditableEntity()
    {
        CreatedAt = DateTime.UtcNow;
    }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    protected void Touch() =>
        UpdatedAt = DateTime.UtcNow;
}

