using Microsoft.EntityFrameworkCore;
using PadelBooking.Application.Common;
using PadelBooking.Application.DTOs.PriceRules;
using PadelBooking.Application.Interfaces;
using PadelBooking.Domain.Entities;

namespace PadelBooking.Application.Services;

public class PriceRuleService : IPriceRuleService
{
    private readonly IAppDbContext _db;

    public PriceRuleService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<List<PriceRuleDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var rules = await _db.PriceRules.ToListAsync(cancellationToken);

        return rules
            .OrderBy(p => p.DayOfWeek)
            .ThenBy(p => p.StartTime)
            .Select(ToDto)
            .ToList();
    }

    public async Task<Result<PriceRuleDto>> CreateAsync(CreatePriceRuleRequest request, CancellationToken cancellationToken = default)
    {
        var validation = ValidateTimes(request.StartTime, request.EndTime);
        if (validation is not null)
        {
            return Result<PriceRuleDto>.Failure(validation);
        }

        if (request.IsDefault)
        {
            // Only one default rule is allowed — clear any existing default.
            var currentDefaults = await _db.PriceRules.Where(p => p.IsDefault).ToListAsync(cancellationToken);
            foreach (var d in currentDefaults) d.IsDefault = false;
        }

        var rule = new PriceRule
        {
            Name = request.Name,
            DayOfWeek = request.DayOfWeek,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            PricePerHour = request.PricePerHour,
            IsDefault = request.IsDefault,
            IsActive = true
        };

        _db.PriceRules.Add(rule);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<PriceRuleDto>.Success(ToDto(rule));
    }

    public async Task<Result<PriceRuleDto>> UpdateAsync(int id, UpdatePriceRuleRequest request, CancellationToken cancellationToken = default)
    {
        var validation = ValidateTimes(request.StartTime, request.EndTime);
        if (validation is not null)
        {
            return Result<PriceRuleDto>.Failure(validation);
        }

        var rule = await _db.PriceRules.FindAsync(new object?[] { id }, cancellationToken);
        if (rule is null)
        {
            return Result<PriceRuleDto>.Failure($"Price rule with id {id} was not found.", ResultErrorType.NotFound);
        }

        if (request.IsDefault && !rule.IsDefault)
        {
            var currentDefaults = await _db.PriceRules.Where(p => p.IsDefault && p.Id != id).ToListAsync(cancellationToken);
            foreach (var d in currentDefaults) d.IsDefault = false;
        }

        rule.Name = request.Name;
        rule.DayOfWeek = request.DayOfWeek;
        rule.StartTime = request.StartTime;
        rule.EndTime = request.EndTime;
        rule.PricePerHour = request.PricePerHour;
        rule.IsDefault = request.IsDefault;
        rule.IsActive = request.IsActive;

        await _db.SaveChangesAsync(cancellationToken);

        return Result<PriceRuleDto>.Success(ToDto(rule));
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var rule = await _db.PriceRules.FindAsync(new object?[] { id }, cancellationToken);
        if (rule is null)
        {
            return Result<bool>.Failure($"Price rule with id {id} was not found.", ResultErrorType.NotFound);
        }

        _db.PriceRules.Remove(rule);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

    private static string? ValidateTimes(TimeSpan start, TimeSpan end) =>
        end <= start ? "EndTime must be after StartTime." : null;

    private static PriceRuleDto ToDto(PriceRule p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        DayOfWeek = p.DayOfWeek,
        StartTime = p.StartTime,
        EndTime = p.EndTime,
        PricePerHour = p.PricePerHour,
        IsDefault = p.IsDefault,
        IsActive = p.IsActive
    };
}
