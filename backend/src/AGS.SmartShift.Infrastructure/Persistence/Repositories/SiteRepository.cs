using AGS.SmartShift.Domain.Entities.Identity;
using AGS.SmartShift.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AGS.SmartShift.Infrastructure.Persistence.Repositories;

public sealed class SiteRepository : ISiteRepository
{
    private readonly SmartShiftDbContext _db;

    public SiteRepository(SmartShiftDbContext db) => _db = db;

    public async Task<IReadOnlyList<Site>> ListAsync(CancellationToken cancellationToken = default) =>
        await _db.Sites.AsNoTracking().OrderBy(s => s.Code).ToListAsync(cancellationToken);

    public Task<Site?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _db.Sites.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
}
