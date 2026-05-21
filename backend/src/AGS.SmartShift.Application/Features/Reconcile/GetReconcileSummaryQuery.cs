using AGS.SmartShift.Application.Contracts.Reconcile;
using MediatR;

namespace AGS.SmartShift.Application.Features.Reconcile;

public sealed record GetReconcileSummaryQuery(string? WeekId, string DepartmentCode)
    : IRequest<ReconcileSummaryDto>;
