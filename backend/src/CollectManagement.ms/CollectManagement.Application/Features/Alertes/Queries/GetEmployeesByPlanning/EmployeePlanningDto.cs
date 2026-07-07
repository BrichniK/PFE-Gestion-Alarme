namespace CollectManagement.Application.Features.Alertes.Queries.GetEmployeesByPlanning;

public record EmployeePlanningDto(
    Ulid EmployeeId,
    string Nom,
    string Prenom,
    int Phone,
    string? Email
);
