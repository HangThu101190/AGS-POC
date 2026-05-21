using AGS.SmartShift.Application.Contracts.Planning;
using MediatR;

namespace AGS.SmartShift.Application.Features.SupBoard;

public sealed record GetSupBoardQuery(string? WeekId, int? DayIdx, string? DepartmentCode)
    : IRequest<SupBoardDto>;
