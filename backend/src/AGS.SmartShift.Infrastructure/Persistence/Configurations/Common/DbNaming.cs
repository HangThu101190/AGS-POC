namespace AGS.SmartShift.Infrastructure.Persistence.Configurations.Common;

/// <summary>
/// Physical PostgreSQL names. All <c>timestamptz</c> values are stored in UTC; columns use <c>*_at</c>, never <c>*_utc</c>.
/// </summary>
internal static class DbNaming
{
    internal static class Tables
    {
        public const string Users = "users";
        public const string Notifications = "notifications";
        public const string AttendanceLocationSamples = "attendance_location_samples";
    }

    internal static class Columns
    {
        public const string UserId = "user_id";
        public const string CheckInAt = "check_in_at";
        public const string CheckOutAt = "check_out_at";
        public const string RecordedAt = "recorded_at";
        public const string OccurredAt = "occurred_at";
        public const string ExpiresAt = "expires_at";
        public const string RevokedAt = "revoked_at";
        public const string LockoutEndAt = "lockout_end_at";
        public const string PublishedAt = "published_at";
        public const string LockedAt = "locked_at";
        public const string ConfirmedAt = "confirmed_at";
    }
}
