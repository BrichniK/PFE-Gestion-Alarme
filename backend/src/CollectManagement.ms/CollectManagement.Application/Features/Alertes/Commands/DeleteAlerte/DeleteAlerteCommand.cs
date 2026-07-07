namespace CollectManagement.Application.Features.Alertes.Commands.DeleteAlerte;

public record DeleteAlerteCommand(Ulid AlerteId) : IRequest;
