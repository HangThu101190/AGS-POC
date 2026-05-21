using AGS.SmartShift.Application.Common.Interfaces;

namespace AGS.SmartShift.Infrastructure.Services;

public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
