namespace CollectManagement.Application.Features.Alertes.Queries.GetEmployeesByPlanning;

public record GroupeWithEmployeesDto(
    string GroupeNom,
    string ShiftLabel,
    string ShiftStartTime,
    string ShiftEndTime,
    List<EmployeePlanningDto> Employees
);
