using MediatR;

namespace AGS.SmartShift.Application.Features.Reconcile;

public sealed record ExportReconcileMonthQuery(string? WeekId, string DepartmentCode, string? MonthLabel)
    : IRequest<byte[]>;
