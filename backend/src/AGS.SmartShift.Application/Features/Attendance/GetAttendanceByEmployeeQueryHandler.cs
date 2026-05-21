using AGS.SmartShift.Application.Common.Authorization;
using AGS.SmartShift.Application.Common.Exceptions;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Attendance;

public sealed class GetAttendanceByEmployeeQueryHandler
    : IRequestHandler<GetAttendanceByEmployeeQuery, AttendanceDto?>
{
    private readonly IAttendanceRepository _attendance;
    private readonly IResourceAccessService _access;

    public GetAttendanceByEmployeeQueryHandler(
        IAttendanceRepository attendance,
        IResourceAccessService access)
    {
        _attendance = attendance;
        _access = access;
    }

    public async Task<AttendanceDto?> Handle(
        GetAttendanceByEmployeeQuery request,
        CancellationToken cancellationToken)
    {
        if (!await _access.CanAccessEmployeeAsync(request.EmployeeId, cancellationToken))
        {
            throw new ForbiddenAccessException("Cannot view another employee's attendance record.");
        }

        var record = await _attendance.GetByEmployeeIdAsync(request.EmployeeId, cancellationToken);
        if (record is null)
        {
            return null;
        }

        return AttendanceMapping.ToDto(record);
    }
}
