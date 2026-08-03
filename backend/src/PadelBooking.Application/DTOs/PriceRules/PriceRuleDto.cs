using System.ComponentModel.DataAnnotations;

namespace PadelBooking.Application.DTOs.PriceRules;

public class PriceRuleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DayOfWeek? DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public decimal PricePerHour { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
}

public class CreatePriceRuleRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Null = applies to every day of the week.</summary>
    public DayOfWeek? DayOfWeek { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    [Range(0, 100000)]
    public decimal PricePerHour { get; set; }

    public bool IsDefault { get; set; }
}

public class UpdatePriceRuleRequest : CreatePriceRuleRequest
{
    public bool IsActive { get; set; }
}
