using AGS.SmartShift.Application.Common.Models;
using AGS.SmartShift.Application.Contracts.Audit;
using AGS.SmartShift.Application.Contracts.Common;
using MediatR;

namespace AGS.SmartShift.Application.Features.Audit;

public sealed record ListAuditEventsQuery(
    TableListRequest Table,
    DateTime? FromUtc = null,
    DateTime? ToUtc = null) : IRequest<PagedList<AuditEventDto>>;
