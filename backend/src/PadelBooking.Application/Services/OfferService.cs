using Microsoft.EntityFrameworkCore;
using PadelBooking.Application.Common;
using PadelBooking.Application.DTOs.Offers;
using PadelBooking.Application.Interfaces;
using PadelBooking.Domain.Entities;
using PadelBooking.Domain.Enums;

namespace PadelBooking.Application.Services;

public class OfferService : IOfferService
{
    private readonly IAppDbContext _db;

    public OfferService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<List<OfferDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Offers
            .OrderByDescending(o => o.StartDate)
            .Select(o => ToDto(o))
            .ToListAsync(cancellationToken);
    }

    public async Task<Result<OfferDto>> CreateAsync(CreateOfferRequest request, CancellationToken cancellationToken = default)
    {
        var validation = Validate(request);
        if (validation is not null)
        {
            return Result<OfferDto>.Failure(validation);
        }

        var offer = new Offer
        {
            Name = request.Name,
            Description = request.Description,
            DiscountType = request.DiscountType,
            DiscountValue = request.DiscountValue,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            DayOfWeek = request.DayOfWeek,
            IsActive = true
        };

        _db.Offers.Add(offer);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<OfferDto>.Success(ToDto(offer));
    }

    public async Task<Result<OfferDto>> UpdateAsync(int id, UpdateOfferRequest request, CancellationToken cancellationToken = default)
    {
        var validation = Validate(request);
        if (validation is not null)
        {
            return Result<OfferDto>.Failure(validation);
        }

        var offer = await _db.Offers.FindAsync(new object?[] { id }, cancellationToken);
        if (offer is null)
        {
            return Result<OfferDto>.Failure($"Offer with id {id} was not found.", ResultErrorType.NotFound);
        }

        offer.Name = request.Name;
        offer.Description = request.Description;
        offer.DiscountType = request.DiscountType;
        offer.DiscountValue = request.DiscountValue;
        offer.StartDate = request.StartDate;
        offer.EndDate = request.EndDate;
        offer.DayOfWeek = request.DayOfWeek;
        offer.IsActive = request.IsActive;

        await _db.SaveChangesAsync(cancellationToken);

        return Result<OfferDto>.Success(ToDto(offer));
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var offer = await _db.Offers.FindAsync(new object?[] { id }, cancellationToken);
        if (offer is null)
        {
            return Result<bool>.Failure($"Offer with id {id} was not found.", ResultErrorType.NotFound);
        }

        _db.Offers.Remove(offer);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

    private static string? Validate(CreateOfferRequest request)
    {
        if (request.EndDate < request.StartDate)
        {
            return "EndDate must be on or after StartDate.";
        }

        if (request.DiscountType == DiscountType.Percentage && (request.DiscountValue <= 0 || request.DiscountValue > 100))
        {
            return "Percentage discounts must be between 0 and 100.";
        }

        if (request.DiscountType == DiscountType.FixedAmount && request.DiscountValue <= 0)
        {
            return "Fixed discount amount must be greater than 0.";
        }

        return null;
    }

    private static OfferDto ToDto(Offer o) => new()
    {
        Id = o.Id,
        Name = o.Name,
        Description = o.Description,
        DiscountType = o.DiscountType,
        DiscountValue = o.DiscountValue,
        StartDate = o.StartDate,
        EndDate = o.EndDate,
        DayOfWeek = o.DayOfWeek,
        IsActive = o.IsActive
    };
}
