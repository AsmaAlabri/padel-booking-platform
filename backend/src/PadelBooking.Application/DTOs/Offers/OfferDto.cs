using System.ComponentModel.DataAnnotations;
using PadelBooking.Domain.Enums;

namespace PadelBooking.Application.DTOs.Offers;

public class OfferDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public DayOfWeek? DayOfWeek { get; set; }
    public bool IsActive { get; set; }
}

public class CreateOfferRequest
{
    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public DiscountType DiscountType { get; set; }

    [Range(0, 100000)]
    public decimal DiscountValue { get; set; }

    [Required]
    public DateOnly StartDate { get; set; }

    [Required]
    public DateOnly EndDate { get; set; }

    public DayOfWeek? DayOfWeek { get; set; }
}

public class UpdateOfferRequest : CreateOfferRequest
{
    public bool IsActive { get; set; }
}
