namespace CollectManagement.Application.Features.Types.Commands.UpdateType;

public record UpdateTypeCommand(
    Ulid TypeId,
    string Code,
    string Label,
    int? DureeNominal
) : IRequest;
