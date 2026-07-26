using CollectManagement.Domain.Common;
using CollectManagement.Domain.Employess;
using CollectManagement.Domain.Employess.ObjectValues;
using CollectManagement.Domain.Plannings.ValueObjects;

namespace CollectManagement.Domain.Plannings;

public class PlanningEmployee : AuditableEntity
{
    public PlanningId PlanningId { get; private set; }

    public Planning Planning { get; private set; }

    public EmployeeId EmployeeId { get; private set; }

    public Employee Employee { get; private set; }

    private PlanningEmployee(
        PlanningId planningId,
        EmployeeId employeeId)
    {
        PlanningId = planningId;
        EmployeeId = employeeId;
    }

    public static PlanningEmployee Create(
        PlanningId planningId,
        EmployeeId employeeId)
    {
        return new PlanningEmployee(planningId, employeeId);
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private PlanningEmployee() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
