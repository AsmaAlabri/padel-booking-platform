namespace PadelBooking.Application.DTOs.Availability;

/// <summary>
/// A single bookable hour on the requested date. Deliberately contains no court
/// identity or count — customers only ever see whether an hour can be booked.
/// </summary>
public class AvailabilitySlotDto
{
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public decimal PricePerHour { get; set; }
    public bool IsAvailable { get; set; }
}

public class DailyAvailabilityDto
{
    public DateOnly Date { get; set; }
    public List<AvailabilitySlotDto> Slots { get; set; } = new();
}
