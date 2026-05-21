using AGS.SmartShift.Domain.Entities.Planning;

namespace AGS.SmartShift.Domain.Repositories;

public interface IFlightImportJobRepository
{
    Task AddAsync(FlightImportJob job, CancellationToken cancellationToken = default);

    Task<FlightImportJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
