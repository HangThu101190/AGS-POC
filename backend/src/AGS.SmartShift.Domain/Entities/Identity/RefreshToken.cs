using AGS.SmartShift.Domain.Common;

namespace AGS.SmartShift.Domain.Entities.Identity;

public sealed class RefreshToken : Entity<Guid>
{
    public Guid UserAccountId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }

    private RefreshToken()
    {
    }

    public static RefreshToken Create(
        Guid userAccountId,
        string tokenHash,
        DateTime expiresAtUtc,
        DateTime utcNow)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserAccountId = userAccountId,
            TokenHash = tokenHash,
            ExpiresAtUtc = expiresAtUtc,
        };
    }

    public bool IsActive(DateTime utcNow) =>
        RevokedAtUtc is null && ExpiresAtUtc > utcNow;

    public void Revoke(DateTime utcNow) => RevokedAtUtc = utcNow;
}
