using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Entities.Audit;

namespace AGS.SmartShift.Domain.Repositories;

public interface IAuditEventRepository
{
    Task<PagedResult<AuditEventListRow>> ListAsync(
        TableListCriteria criteria,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        CancellationToken cancellationToken = default);
}

public sealed record AuditEventListRow(
    AuditEvent Event,
    string? ActorName);
