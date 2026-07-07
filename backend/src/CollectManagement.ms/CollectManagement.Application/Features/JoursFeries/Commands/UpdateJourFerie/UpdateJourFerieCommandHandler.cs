using CollectManagement.Application.Interfaces.Repositories.JoursFeries;
using CollectManagement.Domain.JoursFeries;
using CollectManagement.Domain.JoursFeries.ValueObjects;

namespace CollectManagement.Application.Features.JoursFeries.Commands.UpdateJourFerie;

public class UpdateJourFerieCommandHandler
    : IRequestHandler<UpdateJourFerieCommand>
{
    private readonly IJourFerieRepository _jourFerieRepository;

    public UpdateJourFerieCommandHandler(IJourFerieRepository jourFerieRepository)
    {
        _jourFerieRepository = jourFerieRepository;
    }

    public async Task Handle(UpdateJourFerieCommand request, CancellationToken cancellationToken)
    {
        var jourFerieId = new JourFerieId(request.JourFerieId);

        var jourFerie = JourFerie.Create(
            jourFerieId,
            request.Date,
            request.Label
        );

        await _jourFerieRepository.UpdateBulkAsync(jourFerie, cancellationToken)
            .ConfigureAwait(false);
    }
}
