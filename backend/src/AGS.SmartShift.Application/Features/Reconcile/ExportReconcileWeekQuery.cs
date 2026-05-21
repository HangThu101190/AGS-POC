using MediatR;

namespace AGS.SmartShift.Application.Features.Reconcile;

public sealed record ExportReconcileWeekQuery(string? WeekId, string DepartmentCode) : IRequest<byte[]>;
