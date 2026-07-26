namespace CollectManagement.Application.Features.Groupes.Commands.CreateGroupe;

public record CreateGroupeCommand(
    string Nom,
    string Color,
    List<Ulid> EmployeeIds
) : IRequest<CreateGroupeResponse>;
