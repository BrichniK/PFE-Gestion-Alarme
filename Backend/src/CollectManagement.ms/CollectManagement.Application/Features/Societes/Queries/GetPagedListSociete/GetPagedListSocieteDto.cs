namespace CollectManagement.Application.Features.Societes.Queries.GetPagedListSociete;

public record GetPagedListSocieteDto(
    Ulid SocieteId,
    string LogoPath,
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
    );