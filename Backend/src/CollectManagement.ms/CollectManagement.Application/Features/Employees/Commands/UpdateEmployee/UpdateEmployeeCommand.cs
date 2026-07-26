using MediatR;

namespace CollectManagement.Application.Features.Employees.Commands.UpdateEmployee;

public record UpdateEmployeeCommand( 
    Ulid EmployeeId,
    string Nom,
    string Prenom,
    int Phone,
    string Rfid,
    string? Email,
    string? LogoPath,
    string? LogoData,
    string? LogoExtension
) : IRequest;