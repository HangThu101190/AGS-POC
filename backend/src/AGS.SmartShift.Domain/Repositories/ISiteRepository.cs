using AGS.SmartShift.Domain.Entities.Identity;

namespace AGS.SmartShift.Domain.Repositories;

public interface ISiteRepository
{
    Task<IReadOnlyList<Site>> ListAsync(CancellationToken cancellationToken = default);

    Task<Site?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
