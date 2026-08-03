namespace PadelBooking.Domain.Common;

/// <summary>
/// Base class for all domain entities. Provides a strongly-typed primary key
/// and standard audit timestamps.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
