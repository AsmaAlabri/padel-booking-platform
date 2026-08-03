namespace PadelBooking.Infrastructure.ExternalServices.Thawani;

public class ThawaniOptions
{
    public string BaseUrl { get; set; } = "https://uatcheckout.thawani.om/api/v1";
    public string PublishableKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>Where Thawani's checkout page is hosted — used to build the pay URL from a session_id.</summary>
    public string CheckoutBaseUrl { get; set; } = "https://uatcheckout.thawani.om/pay";

    public string SuccessUrl { get; set; } = "http://localhost:5173/payment/callback?status=success";
    public string CancelUrl { get; set; } = "http://localhost:5173/payment/callback?status=cancelled";
}
