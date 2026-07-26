namespace CollectManagement.Application.Features.Groupes.Queries.GetPagedListGroupe;

public record GetPagedListGroupeDto(
    Ulid GroupeId,
    string Nom,
    string Color,
    List<Ulid> EmployeeIds
);
