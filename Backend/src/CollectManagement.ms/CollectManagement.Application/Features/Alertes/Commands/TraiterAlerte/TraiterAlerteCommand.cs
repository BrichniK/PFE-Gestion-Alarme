namespace CollectManagement.Application.Features.Alertes.Commands.TraiterAlerte;

public record TraiterAlerteCommand(
    Ulid AlerteId,
    Ulid EmployeeId
) : IRequest<bool>;
