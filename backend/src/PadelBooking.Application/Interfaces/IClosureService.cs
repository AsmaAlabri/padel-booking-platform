using PadelBooking.Application.Common;
using PadelBooking.Application.DTOs.Closures;

namespace PadelBooking.Application.Interfaces;

public interface IClosureService
{
    Task<List<ClosureDto>> GetAllAsync(DateOnly? fromDate = null, CancellationToken cancellationToken = default);
    Task<Result<ClosureDto>> CreateAsync(CreateClosureRequest request, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
