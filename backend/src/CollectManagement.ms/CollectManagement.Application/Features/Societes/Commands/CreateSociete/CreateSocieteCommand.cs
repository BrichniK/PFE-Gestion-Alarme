namespace CollectManagement.Application.Features.Societes.Commands.CreateSociete;

public record CreateSocieteCommand(
    string LogoPath,
    string? LogoData,
    string? LogoExtension,
    string Nom,
    string? MatriculeFiscal,
    string? Rne,
    decimal? Capital,
    DateTime DateOverture,
    string? Telephone1,
    string? Telephone2,
    string? Fax1,
    string? Fax2,
    string? Email,
    string? Adresse,
    string? CodeSociete
    ):IRequest<CreateSocieteReponse>;