using AGS.SmartShift.Application.Common.Exceptions;
using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Attendance;

public sealed class CheckOutCommandHandler : IRequestHandler<CheckOutCommand, AttendanceDto>
{
    private readonly IAttendanceRepository _attendance;
    private readonly ICurrentUserService _user;
    private readonly IDateTimeProvider _clock;

    public CheckOutCommandHandler(
        IAttendanceRepository attendance,
        ICurrentUserService user,
        IDateTimeProvider clock)
    {
        _attendance = attendance;
        _user = user;
        _clock = clock;
    }

    public async Task<AttendanceDto> Handle(CheckOutCommand request, CancellationToken cancellationToken)
    {
        if (_user.EmployeeId is not Guid employeeId)
        {
            throw new ForbiddenAccessException("Staff account required for check-out.");
        }

        var now = _clock.UtcNow;
        var record = await _attendance.GetTrackedByEmployeeIdAsync(employeeId, cancellationToken)
            ?? throw new DomainException("not_checked_in", "Chưa check-in — không thể check-out.");

        if (!record.IsActive)
        {
            throw new DomainException("already_checked_out", "Đã check-out cho ca hiện tại.");
        }

        record.ApplyCheckOut(now, request.EarlyNote);
        await _attendance.SaveChangesAsync(cancellationToken);

        return AttendanceMapping.ToDto(record);
    }
}
