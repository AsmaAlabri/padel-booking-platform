namespace PadelBooking.Application.DTOs.Payments;

public class ThawaniCheckoutRequest
{
    public string BookingReference { get; set; } = string.Empty;
    public decimal AmountOmr { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
}

public class ThawaniSessionResult
{
    public string SessionId { get; set; } = string.Empty;
    public string CheckoutUrl { get; set; } = string.Empty;
}

public class InitiatePaymentResponse
{
    public string CheckoutUrl { get; set; } = string.Empty;
}

public class PaymentStatusResponse
{
    public string BookingReference { get; set; } = string.Empty;
    public string BookingStatus { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
}
