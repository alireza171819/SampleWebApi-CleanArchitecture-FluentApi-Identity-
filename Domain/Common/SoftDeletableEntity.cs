namespace Domain.Common;

public abstract class SoftDeletableEntity : AuditableEntity
{
    protected SoftDeletableEntity() { }

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public void Delete()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        Touch();
    }
}
