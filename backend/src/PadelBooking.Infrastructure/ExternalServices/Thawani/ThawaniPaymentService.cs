using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using PadelBooking.Application.Common;
using PadelBooking.Application.DTOs.Payments;
using PadelBooking.Application.Interfaces;

namespace PadelBooking.Infrastructure.ExternalServices.Thawani;

/// <summary>
/// Talks to the real Thawani Checkout API (https://developer.thawani.om).
/// Amounts are sent as integer baisas — Thawani does not accept decimals
/// (1 OMR = 1000 baisas), so OMR values are multiplied by 1000 and rounded.
/// Requires real UAT sandbox credentials in appsettings ("Thawani" section)
/// to actually exchange traffic with Thawani — see README.
/// </summary>
public class ThawaniPaymentService : IThawaniPaymentService
{
    private readonly HttpClient _httpClient;
    private readonly ThawaniOptions _options;

    public ThawaniPaymentService(HttpClient httpClient, IOptions<ThawaniOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;

        _httpClient.BaseAddress = new Uri(_options.BaseUrl.TrimEnd('/') + "/");
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (!_httpClient.DefaultRequestHeaders.Contains("thawani-api-key"))
        {
            _httpClient.DefaultRequestHeaders.Add("thawani-api-key", _options.SecretKey);
        }
    }

    public async Task<Result<ThawaniSessionResult>> CreateCheckoutSessionAsync(
        ThawaniCheckoutRequest request, CancellationToken cancellationToken = default)
    {
        var unitAmountBaisas = (int)Math.Round(request.AmountOmr * 1000m, MidpointRounding.AwayFromZero);

        var payload = new ThawaniCreateSessionRequest
        {
            ClientReferenceId = request.BookingReference,
            Mode = "payment",
            Products = new List<ThawaniProduct>
            {
                new()
                {
                    Name = $"Padel court booking {request.BookingReference}",
                    UnitAmount = unitAmountBaisas,
                    Quantity = 1
                }
            },
            SuccessUrl = AppendReference(_options.SuccessUrl, request.BookingReference),
            CancelUrl = AppendReference(_options.CancelUrl, request.BookingReference),
            Metadata = new Dictionary<string, string>
            {
                ["customer_name"] = request.CustomerName,
                ["customer_phone"] = request.CustomerPhone,
                ["customer_email"] = request.CustomerEmail ?? string.Empty,
                ["booking_reference"] = request.BookingReference
            }
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync("checkout/session", payload, cancellationToken);
            var body = await response.Content.ReadFromJsonAsync<ThawaniSessionEnvelope>(cancellationToken: cancellationToken);

            if (!response.IsSuccessStatusCode || body?.Data?.SessionId is null)
            {
                return Result<ThawaniSessionResult>.Failure(
                    body?.Description ?? "Failed to create Thawani checkout session.", ResultErrorType.Conflict);
            }

            var checkoutUrl = $"{_options.CheckoutBaseUrl.TrimEnd('/')}/{body.Data.SessionId}?key={_options.PublishableKey}";

            return Result<ThawaniSessionResult>.Success(new ThawaniSessionResult
            {
                SessionId = body.Data.SessionId,
                CheckoutUrl = checkoutUrl
            });
        }
        catch (HttpRequestException ex)
        {
            return Result<ThawaniSessionResult>.Failure(
                $"Could not reach Thawani: {ex.Message}", ResultErrorType.Conflict);
        }
    }

    public async Task<Result<string>> GetSessionPaymentStatusAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"checkout/session/{sessionId}", cancellationToken);
            var body = await response.Content.ReadFromJsonAsync<ThawaniSessionEnvelope>(cancellationToken: cancellationToken);

            if (!response.IsSuccessStatusCode || body?.Data is null)
            {
                return Result<string>.Failure(
                    body?.Description ?? "Failed to retrieve Thawani session status.", ResultErrorType.Conflict);
            }

            // One of: "paid", "unpaid", "cancelled" per Thawani's documented session status values.
            return Result<string>.Success(body.Data.PaymentStatus ?? "unpaid");
        }
        catch (HttpRequestException ex)
        {
            return Result<string>.Failure($"Could not reach Thawani: {ex.Message}", ResultErrorType.Conflict);
        }
    }

    private static string AppendReference(string url, string bookingReference)
    {
        var separator = url.Contains('?') ? "&" : "?";
        return $"{url}{separator}reference={Uri.EscapeDataString(bookingReference)}";
    }

    private class ThawaniProduct
    {
        [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
        [JsonPropertyName("unit_amount")] public int UnitAmount { get; set; }
        [JsonPropertyName("quantity")] public int Quantity { get; set; }
    }

    private class ThawaniCreateSessionRequest
    {
        [JsonPropertyName("client_reference_id")] public string ClientReferenceId { get; set; } = string.Empty;
        [JsonPropertyName("mode")] public string Mode { get; set; } = "payment";
        [JsonPropertyName("products")] public List<ThawaniProduct> Products { get; set; } = new();
        [JsonPropertyName("success_url")] public string SuccessUrl { get; set; } = string.Empty;
        [JsonPropertyName("cancel_url")] public string CancelUrl { get; set; } = string.Empty;
        [JsonPropertyName("metadata")] public Dictionary<string, string>? Metadata { get; set; }
    }

    private class ThawaniSessionEnvelope
    {
        [JsonPropertyName("success")] public bool Success { get; set; }
        [JsonPropertyName("code")] public string? Code { get; set; }
        [JsonPropertyName("description")] public string? Description { get; set; }
        [JsonPropertyName("data")] public ThawaniSessionData? Data { get; set; }
    }

    private class ThawaniSessionData
    {
        [JsonPropertyName("session_id")] public string? SessionId { get; set; }
        [JsonPropertyName("client_reference_id")] public string? ClientReferenceId { get; set; }
        [JsonPropertyName("payment_status")] public string? PaymentStatus { get; set; }
    }
}
