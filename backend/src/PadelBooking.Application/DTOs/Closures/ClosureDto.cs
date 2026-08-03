using System.ComponentModel.DataAnnotations;

namespace PadelBooking.Application.DTOs.Closures;

public class ClosureDto
{
    public int Id { get; set; }
    public int? CourtId { get; set; }
    public string? CourtName { get; set; }
    public DateOnly Date { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public string? Reason { get; set; }
}

public class CreateClosureRequest
{
    /// <summary>Null = closure applies to all courts.</summary>
    public int? CourtId { get; set; }

    [Required]
    public DateOnly Date { get; set; }

    /// <summary>Leave both null for a full-day closure.</summary>
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }

    [MaxLength(300)]
    public string? Reason { get; set; }
}
