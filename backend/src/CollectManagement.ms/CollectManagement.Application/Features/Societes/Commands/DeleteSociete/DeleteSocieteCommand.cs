namespace CollectManagement.Application.Features.Societes.Commands.DeleteSociete;

public record DeleteSocieteCommand(
    Ulid SocieteId
    ):IRequest;