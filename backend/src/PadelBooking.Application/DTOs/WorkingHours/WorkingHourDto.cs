using System.ComponentModel.DataAnnotations;

namespace PadelBooking.Application.DTOs.WorkingHours;

public class WorkingHourDto
{
    public int Id { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan OpenTime { get; set; }
    public TimeSpan CloseTime { get; set; }
    public bool IsClosed { get; set; }
}

public class UpdateWorkingHourRequest
{
    [Required]
    public TimeSpan OpenTime { get; set; }

    [Required]
    public TimeSpan CloseTime { get; set; }

    public bool IsClosed { get; set; }
}
