using AGS.SmartShift.Domain.Entities.Attendance;
using AGS.SmartShift.Domain.Entities.Audit;
using AGS.SmartShift.Domain.Entities.Identity;
using AGS.SmartShift.Domain.Entities.Leave;
using AGS.SmartShift.Domain.Entities.Planning;
using Microsoft.EntityFrameworkCore;

namespace AGS.SmartShift.Infrastructure.Persistence;

public sealed class SmartShiftDbContext : DbContext
{
    public SmartShiftDbContext(DbContextOptions<SmartShiftDbContext> options)
        : base(options)
    {
    }

    public DbSet<Site> Sites => Set<Site>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();
    public DbSet<SystemRole> SystemRoles => Set<SystemRole>();
    public DbSet<SystemPermission> SystemPermissions => Set<SystemPermission>();
    public DbSet<SystemRolePermission> SystemRolePermissions => Set<SystemRolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<AttendanceGpsPoint> AttendanceGpsPoints => Set<AttendanceGpsPoint>();
    public DbSet<WorkZone> WorkZones => Set<WorkZone>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
    public DbSet<Flight> Flights => Set<Flight>();
    public DbSet<WeeklyPlan> WeeklyPlans => Set<WeeklyPlan>();
    public DbSet<ShiftSlot> ShiftSlots => Set<ShiftSlot>();
    public DbSet<NotificationMessage> NotificationMessages => Set<NotificationMessage>();
    public DbSet<FlightSchedule> FlightSchedules => Set<FlightSchedule>();
    public DbSet<FlightScheduleDay> FlightScheduleDays => Set<FlightScheduleDay>();
    public DbSet<ShiftAssignment> ShiftAssignments => Set<ShiftAssignment>();
    public DbSet<ShiftSyncProposal> ShiftSyncProposals => Set<ShiftSyncProposal>();
    public DbSet<ShiftRevision> ShiftRevisions => Set<ShiftRevision>();
    public DbSet<DailyStaffingPlan> DailyStaffingPlans => Set<DailyStaffingPlan>();
    public DbSet<DailyStaffingLine> DailyStaffingLines => Set<DailyStaffingLine>();
    public DbSet<FlightCrewAssignment> FlightCrewAssignments => Set<FlightCrewAssignment>();
    public DbSet<AircraftManningRule> AircraftManningRules => Set<AircraftManningRule>();
    public DbSet<AirlineManningRule> AirlineManningRules => Set<AirlineManningRule>();
    public DbSet<EmployeeDayAvailability> EmployeeDayAvailabilities => Set<EmployeeDayAvailability>();
    public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<FlightImportJob> FlightImportJobs => Set<FlightImportJob>();
    public DbSet<ShiftCheckInPolicy> ShiftCheckInPolicies => Set<ShiftCheckInPolicy>();
    public DbSet<ShiftTemplate> ShiftTemplates => Set<ShiftTemplate>();
    public DbSet<EmployeeQualification> EmployeeQualifications => Set<EmployeeQualification>();
    public DbSet<StaffingCrewProposal> StaffingCrewProposals => Set<StaffingCrewProposal>();
    public DbSet<DailyStaffingBioHeader> DailyStaffingBioHeaders => Set<DailyStaffingBioHeader>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SmartShiftDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
