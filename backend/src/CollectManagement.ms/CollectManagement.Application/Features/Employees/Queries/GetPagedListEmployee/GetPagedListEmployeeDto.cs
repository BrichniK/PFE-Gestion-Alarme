namespace CollectManagement.Application.Features.Employees.Queries.GetPagedListEmployee;

public record GetPagedListEmployeeDto(
    Ulid EmployeeId,
    string Nom,
    string Prenom,
    int Phone,
    string Rfid,
    string? Email,
    string? LogoPath
);
