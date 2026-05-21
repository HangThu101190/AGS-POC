using AGS.SmartShift.Application.Common.Interfaces;
using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Entities.Leave;
using AGS.SmartShift.Domain.Repositories;
using MediatR;

namespace AGS.SmartShift.Application.Features.Leave;

public sealed record LeaveRequestDto(
    Guid Id,
    Guid EmployeeId,
    Guid LeaveTypeId,
    DateOnly FromDate,
    DateOnly ToDate,
    string Status,
    string? Note);

public sealed record CreateLeaveRequestDto(
    Guid EmployeeId,
    Guid LeaveTypeId,
    DateOnly FromDate,
    DateOnly ToDate,
    string? Note);

public sealed record LeaveTypeDto(Guid Id, string Code, string Name);

public sealed record ListLeaveTypesQuery : IRequest<IReadOnlyList<LeaveTypeDto>>;

public sealed class ListLeaveTypesQueryHandler : IRequestHandler<ListLeaveTypesQuery, IReadOnlyList<LeaveTypeDto>>
{
    private readonly ILeaveRequestRepository _leave;

    public ListLeaveTypesQueryHandler(ILeaveRequestRepository leave) => _leave = leave;

    public async Task<IReadOnlyList<LeaveTypeDto>> Handle(
        ListLeaveTypesQuery request,
        CancellationToken cancellationToken)
    {
        var types = await _leave.ListLeaveTypesAsync(cancellationToken);
        return types.Select(t => new LeaveTypeDto(t.Id, t.Code, t.Name)).ToList();
    }
}

public sealed record ListLeaveRequestsQuery : IRequest<IReadOnlyList<LeaveRequestDto>>;

public sealed class ListLeaveRequestsQueryHandler : IRequestHandler<ListLeaveRequestsQuery, IReadOnlyList<LeaveRequestDto>>
{
    private readonly ILeaveRequestRepository _leave;

    public ListLeaveRequestsQueryHandler(ILeaveRequestRepository leave) => _leave = leave;

    public async Task<IReadOnlyList<LeaveRequestDto>> Handle(
        ListLeaveRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var rows = await _leave.ListRecentAsync(cancellationToken: cancellationToken);
        return rows.Select(Map).ToList();
    }

    private static LeaveRequestDto Map(LeaveRequest r) =>
        new(r.Id, r.EmployeeId, r.LeaveTypeId, r.FromDate, r.ToDate, r.Status.ToString(), r.Note);
}

public sealed record CreateLeaveRequestCommand(CreateLeaveRequestDto Body) : IRequest<LeaveRequestDto>;

public sealed class CreateLeaveRequestCommandHandler : IRequestHandler<CreateLeaveRequestCommand, LeaveRequestDto>
{
    private readonly ILeaveRequestRepository _leave;
    private readonly IDateTimeProvider _clock;

    public CreateLeaveRequestCommandHandler(ILeaveRequestRepository leave, IDateTimeProvider clock)
    {
        _leave = leave;
        _clock = clock;
    }

    public async Task<LeaveRequestDto> Handle(CreateLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var entity = LeaveRequest.Create(
            request.Body.EmployeeId,
            request.Body.LeaveTypeId,
            request.Body.FromDate,
            request.Body.ToDate,
            request.Body.Note,
            _clock.UtcNow);
        await _leave.AddAsync(entity, cancellationToken);
        await _leave.SaveChangesAsync(cancellationToken);
        return new LeaveRequestDto(
            entity.Id,
            entity.EmployeeId,
            entity.LeaveTypeId,
            entity.FromDate,
            entity.ToDate,
            entity.Status.ToString(),
            entity.Note);
    }
}

public sealed record ApproveLeaveRequestCommand(Guid Id) : IRequest<LeaveRequestDto>;

public sealed class ApproveLeaveRequestCommandHandler : IRequestHandler<ApproveLeaveRequestCommand, LeaveRequestDto>
{
    private readonly ILeaveRequestRepository _leave;
    private readonly IDateTimeProvider _clock;

    public ApproveLeaveRequestCommandHandler(ILeaveRequestRepository leave, IDateTimeProvider clock)
    {
        _leave = leave;
        _clock = clock;
    }

    public async Task<LeaveRequestDto> Handle(ApproveLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var entity = await _leave.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new DomainException("leave_not_found", "Không tìm thấy đơn phép.");
        entity.Approve(_clock.UtcNow);
        await _leave.SaveChangesAsync(cancellationToken);
        return new LeaveRequestDto(
            entity.Id,
            entity.EmployeeId,
            entity.LeaveTypeId,
            entity.FromDate,
            entity.ToDate,
            entity.Status.ToString(),
            entity.Note);
    }
}
