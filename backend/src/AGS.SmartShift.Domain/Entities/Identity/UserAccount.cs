using AGS.SmartShift.Domain.Common;

namespace AGS.SmartShift.Domain.Entities.Identity;

public sealed class UserAccount : AuditableEntity<Guid>
{
    public const int MaxFailedAttemptsBeforeLockout = 5;
    public static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    public Guid EmployeeId { get; private set; }
    public string LoginName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public int FailedLoginCount { get; private set; }
    public DateTime? LockoutEndUtc { get; private set; }
    public string PreferredLanguage { get; private set; } = "vi";
    public bool MustChangePassword { get; private set; }

    public Employee? Employee { get; private set; }

    private UserAccount()
    {
    }

    public static UserAccount Create(
        Guid employeeId,
        string loginName,
        string passwordHash,
        DateTime utcNow,
        bool mustChangePassword = false)
    {
        var account = new UserAccount
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            LoginName = loginName.Trim().ToUpperInvariant(),
            PasswordHash = passwordHash,
            MustChangePassword = mustChangePassword,
        };
        account.MarkCreated(utcNow);
        return account;
    }

    public void UpdatePreferences(string preferredLanguage, DateTime utcNow)
    {
        var lang = preferredLanguage.Trim().ToLowerInvariant();
        PreferredLanguage = lang is "en" ? "en" : "vi";
        MarkUpdated(utcNow);
    }

    public void ChangePassword(string passwordHash, DateTime utcNow, bool mustChangePassword = false)
    {
        PasswordHash = passwordHash;
        MustChangePassword = mustChangePassword;
        FailedLoginCount = 0;
        LockoutEndUtc = null;
        MarkUpdated(utcNow);
    }

    public void SetMustChangePassword(bool value, DateTime utcNow)
    {
        MustChangePassword = value;
        MarkUpdated(utcNow);
    }

    public void SetActive(bool isActive, DateTime utcNow)
    {
        IsActive = isActive;
        MarkUpdated(utcNow);
    }

    public bool IsLockedOut(DateTime utcNow) =>
        LockoutEndUtc.HasValue && LockoutEndUtc.Value > utcNow;

    public void RecordFailedLogin(DateTime utcNow)
    {
        FailedLoginCount++;
        if (FailedLoginCount >= MaxFailedAttemptsBeforeLockout)
        {
            LockoutEndUtc = utcNow.Add(LockoutDuration);
        }

        MarkUpdated(utcNow);
    }

    public void RecordSuccessfulLogin(DateTime utcNow)
    {
        FailedLoginCount = 0;
        LockoutEndUtc = null;
        MarkUpdated(utcNow);
    }
}
