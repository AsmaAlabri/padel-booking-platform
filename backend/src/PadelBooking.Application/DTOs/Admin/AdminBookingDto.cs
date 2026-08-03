using System.ComponentModel.DataAnnotations;
using PadelBooking.Domain.Enums;

namespace PadelBooking.Application.DTOs.Admin;

public class AdminBookingDto
{
    public int Id { get; set; }
    public string BookingReference { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public DateOnly BookingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int DurationHours { get; set; }
    public int CourtId { get; set; }
    public string CourtName { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public string? OfferApplied { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UpdateBookingStatusRequest
{
    [Required]
    public BookingStatus Status { get; set; }
}
