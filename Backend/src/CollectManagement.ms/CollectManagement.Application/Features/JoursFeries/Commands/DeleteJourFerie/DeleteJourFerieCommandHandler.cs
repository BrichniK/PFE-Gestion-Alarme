using CollectManagement.Application.Interfaces.Repositories.JoursFeries;
using CollectManagement.Domain.JoursFeries.ValueObjects;

namespace CollectManagement.Application.Features.JoursFeries.Commands.DeleteJourFerie;

public class DeleteJourFerieCommandHandler
    : IRequestHandler<DeleteJourFerieCommand>
{
    private readonly IJourFerieRepository _jourFerieRepository;

    public DeleteJourFerieCommandHandler(IJourFerieRepository jourFerieRepository)
    {
        _jourFerieRepository = jourFerieRepository;
    }

    public async Task Handle(DeleteJourFerieCommand request, CancellationToken cancellationToken)
    {
        var jourFerieId = new JourFerieId(request.JourFerieId);

        await _jourFerieRepository
            .DeleteAsync(
                w => w.JourFerieId.Equals(jourFerieId),
                cancellationToken
            )
            .ConfigureAwait(false);
    }
}
