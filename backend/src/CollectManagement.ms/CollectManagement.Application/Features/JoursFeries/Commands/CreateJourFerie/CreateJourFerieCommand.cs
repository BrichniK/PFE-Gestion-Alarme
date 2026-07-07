namespace CollectManagement.Application.Features.JoursFeries.Commands.CreateJourFerie;

public record CreateJourFerieCommand(
    DateTime Date,
    string Label
) : IRequest<CreateJourFerieResponse>;
