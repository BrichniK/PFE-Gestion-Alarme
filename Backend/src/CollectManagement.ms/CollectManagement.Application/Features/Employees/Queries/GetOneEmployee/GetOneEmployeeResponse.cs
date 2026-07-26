namespace CollectManagement.Application.Features.Employees.Queries.GetOneEmployee;

public record GetOneEmployeeResponse(
    Ulid EmployeeId,
    string Nom,
    string Prenom,
    int Phone,
    string Rfid,
    string? Email,
    string? LogoPath
);
