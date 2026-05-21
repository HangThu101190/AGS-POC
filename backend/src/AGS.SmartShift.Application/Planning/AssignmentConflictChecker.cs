using AGS.SmartShift.Domain.Common;
using AGS.SmartShift.Domain.Entities.Planning;
using AGS.SmartShift.Domain.Enums;

namespace AGS.SmartShift.Application.Planning;

public sealed class AssignmentConflictChecker
{
    public void EnsureCanAssign(
        DailyStaffingLine line,
        Flight flight,
        IReadOnlyList<FlightCrewAssignment> existingForEmployee,
        IReadOnlyList<FlightCrewAssignment> lineAssignments,
        TimeOnly workStart,
        TimeOnly workEnd,
        bool isOvertime,
        bool employeeEligible)
    {
        if (!employeeEligible && !isOvertime)
        {
            throw new DomainException(
                "employee_not_eligible",
                "Nhân viên không khả dụng ngày này. Bật OT để gán ngoài roster.");
        }

        foreach (var other in existingForEmployee)
        {
            if (Overlaps(workStart, workEnd, other.WorkStart, other.WorkEnd))
            {
                throw new DomainException(
                    "assignment_overlap",
                    "Nhân viên đã được gán khung giờ trùng.");
            }
        }

        if (lineAssignments.Count >= line.TargetManning && line.TargetManning > 0)
        {
            throw new DomainException(
                "manning_exceeded",
                $"Đã đủ {line.TargetManning} nhân sự cho chuyến.");
        }
    }

    public void EnsureCanUpdateWorkWindow(
        FlightCrewAssignment assignment,
        IReadOnlyList<FlightCrewAssignment> existingForEmployee,
        TimeOnly workStart,
        TimeOnly workEnd)
    {
        foreach (var other in existingForEmployee)
        {
            if (other.Id == assignment.Id)
            {
                continue;
            }

            if (Overlaps(workStart, workEnd, other.WorkStart, other.WorkEnd))
            {
                throw new DomainException(
                    "assignment_overlap",
                    "Nhân viên đã được gán khung giờ trùng.");
            }
        }
    }

    private static bool Overlaps(TimeOnly aStart, TimeOnly aEnd, TimeOnly bStart, TimeOnly bEnd)
    {
        if (aStart <= aEnd && bStart <= bEnd)
        {
            return aStart < bEnd && bStart < aEnd;
        }

        return true;
    }
}
