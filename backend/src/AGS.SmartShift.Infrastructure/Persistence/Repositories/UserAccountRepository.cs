using AGS.SmartShift.Domain.Entities.Identity;
using AGS.SmartShift.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AGS.SmartShift.Infrastructure.Persistence.Repositories;

public sealed class UserAccountRepository : IUserAccountRepository
{
    private readonly SmartShiftDbContext _db;

    public UserAccountRepository(SmartShiftDbContext db) => _db = db;

    public Task<UserAccount?> GetByLoginNameAsync(string loginName, CancellationToken cancellationToken = default) =>
        _db.UserAccounts
            .Include(x => x.Employee)
            .FirstOrDefaultAsync(x => x.LoginName == loginName, cancellationToken);

    public Task<UserAccount?> GetByIdWithEmployeeAsync(Guid userAccountId, CancellationToken cancellationToken = default) =>
        _db.UserAccounts
            .Include(x => x.Employee)
            .FirstOrDefaultAsync(x => x.Id == userAccountId, cancellationToken);

    public Task<UserAccount?> GetTrackedByIdAsync(Guid userAccountId, CancellationToken cancellationToken = default) =>
        _db.UserAccounts
            .Include(x => x.Employee)
            .FirstOrDefaultAsync(x => x.Id == userAccountId, cancellationToken);

    public Task<UserAccount?> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default) =>
        _db.UserAccounts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.EmployeeId == employeeId, cancellationToken);

    public Task<UserAccount?> GetTrackedByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default) =>
        _db.UserAccounts
            .FirstOrDefaultAsync(x => x.EmployeeId == employeeId, cancellationToken);

    public async Task AddAsync(UserAccount account, CancellationToken cancellationToken = default)
    {
        await _db.UserAccounts.AddAsync(account, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, bool>> GetUserActiveByEmployeeIdsAsync(
        IReadOnlyCollection<Guid> employeeIds,
        CancellationToken cancellationToken = default)
    {
        if (employeeIds.Count == 0)
        {
            return new Dictionary<Guid, bool>();
        }

        var rows = await _db.UserAccounts
            .AsNoTracking()
            .Where(u => employeeIds.Contains(u.EmployeeId))
            .Select(u => new { u.EmployeeId, u.IsActive })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(r => r.EmployeeId, r => r.IsActive);
    }

    public Task<RefreshToken?> GetRefreshTokenByHashAsync(string tokenHash, CancellationToken cancellationToken = default) =>
        _db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

    public Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        _db.RefreshTokens.Add(refreshToken);
        return Task.CompletedTask;
    }

    public Task<int> RemoveExpiredRefreshTokensAsync(DateTime utcNow, CancellationToken cancellationToken = default) =>
        _db.RefreshTokens
            .Where(t => t.ExpiresAtUtc <= utcNow || t.RevokedAtUtc != null)
            .ExecuteDeleteAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}
