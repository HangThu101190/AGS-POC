using AGS.SmartShift.Domain.Entities.Identity;

namespace AGS.SmartShift.Domain.Repositories;

public interface IUserAccountRepository
{
    Task<UserAccount?> GetByLoginNameAsync(string loginName, CancellationToken cancellationToken = default);

    Task<UserAccount?> GetByIdWithEmployeeAsync(Guid userAccountId, CancellationToken cancellationToken = default);

    Task<UserAccount?> GetTrackedByIdAsync(Guid userAccountId, CancellationToken cancellationToken = default);

    Task<UserAccount?> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default);

    Task<UserAccount?> GetTrackedByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, bool>> GetUserActiveByEmployeeIdsAsync(
        IReadOnlyCollection<Guid> employeeIds,
        CancellationToken cancellationToken = default);

    Task AddAsync(UserAccount account, CancellationToken cancellationToken = default);

    Task<RefreshToken?> GetRefreshTokenByHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

    Task<int> RemoveExpiredRefreshTokensAsync(DateTime utcNow, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
