namespace CollectManagement.Application.Features.Groupes.Commands.DeleteGroupe;

public record DeleteGroupeCommand(Ulid GroupeId) : IRequest;
