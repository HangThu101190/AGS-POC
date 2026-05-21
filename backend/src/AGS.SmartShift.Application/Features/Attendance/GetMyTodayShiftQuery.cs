using AGS.SmartShift.Application.Contracts.Attendance;
using MediatR;

namespace AGS.SmartShift.Application.Features.Attendance;

public sealed record GetMyTodayShiftQuery(string? WeekId) : IRequest<StaffTodayShiftDto>;
