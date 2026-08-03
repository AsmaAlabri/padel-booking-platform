using PadelBooking.Application.Common;
using PadelBooking.Application.DTOs.PriceRules;

namespace PadelBooking.Application.Interfaces;

public interface IPriceRuleService
{
    Task<List<PriceRuleDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<PriceRuleDto>> CreateAsync(CreatePriceRuleRequest request, CancellationToken cancellationToken = default);
    Task<Result<PriceRuleDto>> UpdateAsync(int id, UpdatePriceRuleRequest request, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
