namespace CollectManagement.Application.Features.JoursFeries.Commands.DeleteJourFerie;

public record DeleteJourFerieCommand(Ulid JourFerieId) : IRequest;
