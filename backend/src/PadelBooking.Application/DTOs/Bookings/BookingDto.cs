using System.ComponentModel.DataAnnotations;
using PadelBooking.Domain.Enums;

namespace PadelBooking.Application.DTOs.Bookings;

public class CreateBookingRequest
{
    [Required]
    public DateOnly Date { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Range(1, 4, ErrorMessage = "Bookings can be 1 to 4 consecutive hours.")]
    public int DurationHours { get; set; } = 1;

    [MaxLength(150)]
    public string? CustomerName { get; set; }

    [Required, Phone, MaxLength(30)]
    public string CustomerPhone { get; set; } = string.Empty;

    [EmailAddress, MaxLength(200)]
    public string? CustomerEmail { get; set; }

    [Required]
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>Optional teammates to invite onto the booking (padel is typically played in groups of up to 4).</summary>
    [MaxLength(3, ErrorMessage = "You can add up to 3 teammates in addition to yourself.")]
    public List<string>? PlayerNames { get; set; }
}

public class BookingDto
{
    public string BookingReference { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public DateOnly BookingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int DurationHours { get; set; }
    public decimal PricePerHourSnapshot { get; set; }
    public decimal TotalPrice { get; set; }
    public string? OfferApplied { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> PlayerNames { get; set; } = new();
}
