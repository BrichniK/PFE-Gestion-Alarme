using MediatR ;
namespace CollectManagement.Application.Features.Employees.Commands.CreateEmployee;

public record CreateEmployeeCommand(
    string Nom,
    string Prenom,
    int Phone,
    string Rfid,
    string? Email,
    string? Logopath,
    string? LogoData,
    string? LogoExtension
    ) : IRequest<CreateEmployeeResponse>;