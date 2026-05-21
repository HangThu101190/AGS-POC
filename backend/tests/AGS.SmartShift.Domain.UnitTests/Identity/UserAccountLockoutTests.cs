using AGS.SmartShift.Domain.Entities.Identity;
using Xunit;

namespace AGS.SmartShift.Domain.UnitTests.Identity;

public sealed class UserAccountLockoutTests
{
    private static UserAccount CreateAccount() =>
        UserAccount.Create(Guid.NewGuid(), "AGS0001", "hash", DateTime.UtcNow);

    [Fact]
    public void IsLockedOut_false_when_no_lockout_end()
    {
        var account = CreateAccount();
        Assert.False(account.IsLockedOut(DateTime.UtcNow));
    }

    [Fact]
    public void RecordFailedLogin_locks_after_five_attempts()
    {
        var account = CreateAccount();
        var now = new DateTime(2026, 5, 18, 10, 0, 0, DateTimeKind.Utc);

        for (var i = 0; i < UserAccount.MaxFailedAttemptsBeforeLockout - 1; i++)
        {
            account.RecordFailedLogin(now);
            Assert.False(account.IsLockedOut(now));
        }

        account.RecordFailedLogin(now);
        Assert.True(account.IsLockedOut(now));
        Assert.True(account.IsLockedOut(now.AddMinutes(14)));
        Assert.False(account.IsLockedOut(now.Add(UserAccount.LockoutDuration)));
    }

    [Fact]
    public void RecordSuccessfulLogin_clears_lockout_and_counter()
    {
        var account = CreateAccount();
        var now = DateTime.UtcNow;

        for (var i = 0; i < UserAccount.MaxFailedAttemptsBeforeLockout; i++)
        {
            account.RecordFailedLogin(now);
        }

        Assert.True(account.IsLockedOut(now));

        account.RecordSuccessfulLogin(now);
        Assert.False(account.IsLockedOut(now));
    }
}
