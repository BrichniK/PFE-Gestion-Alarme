namespace CollectManagement.Application.Features.JoursFeries.Commands.UpdateJourFerie;

public record UpdateJourFerieCommand(
    Ulid JourFerieId,
    DateTime Date,
    string Label
) : IRequest;
