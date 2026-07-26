namespace CollectManagement.Application.Features.Groupes.Queries.GetOneGroupe;

public record GetOneGroupeResponse(
    Ulid GroupeId,
    string Nom,
    string Color,
    List<Ulid> EmployeeIds
);
