using MediatR;

namespace AGS.SmartShift.Application.Features.Attendance;

public sealed record CheckOutCommand(string? EarlyNote) : IRequest<AttendanceDto>;
