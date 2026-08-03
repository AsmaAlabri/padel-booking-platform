using PadelBooking.Application.Common;
using PadelBooking.Application.DTOs.Offers;

namespace PadelBooking.Application.Interfaces;

public interface IOfferService
{
    Task<List<OfferDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<OfferDto>> CreateAsync(CreateOfferRequest request, CancellationToken cancellationToken = default);
    Task<Result<OfferDto>> UpdateAsync(int id, UpdateOfferRequest request, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
