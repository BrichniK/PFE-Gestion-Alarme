namespace CollectManagement.Application.Features.Types.Commands.CreateType;

public record CreateTypeCommand(
    string Code,
    string Label,
    int? DureeNominal
) : IRequest<CreateTypeResponse>;
