namespace CollectManagement.Application.Features.Groupes.Commands.UpdateGroupe;

public record UpdateGroupeCommand(
    Ulid GroupeId,
    string Nom,
    string Color,
    List<Ulid> EmployeeIds
) : IRequest;
