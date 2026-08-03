using PadelBooking.Application.Common;
using PadelBooking.Application.DTOs.Courts;

namespace PadelBooking.Application.Interfaces;

public interface ICourtService
{
    Task<List<CourtDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<CourtDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<CourtDto>> CreateAsync(CreateCourtRequest request, CancellationToken cancellationToken = default);
    Task<Result<CourtDto>> UpdateAsync(int id, UpdateCourtRequest request, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
