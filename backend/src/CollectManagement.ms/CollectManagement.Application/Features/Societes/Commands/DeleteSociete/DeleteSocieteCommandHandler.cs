using CollectManagement.Application.Interfaces.Societes;
using CollectManagementDomain.Societes.ValueObjects;

namespace CollectManagement.Application.Features.Societes.Commands.DeleteSociete;

public class DeleteSocieteCommandHandler
    : IRequestHandler<DeleteSocieteCommand>
{
    private readonly ISocieteRepository _societeRepository;

    public DeleteSocieteCommandHandler(ISocieteRepository societeRepository)
    {
        _societeRepository = societeRepository;
    }

    public async Task Handle(DeleteSocieteCommand request, CancellationToken cancellationToken)
    {
        var societeId = new SocieteId(request.SocieteId);

        await _societeRepository
            .DeleteAsync(
                w => w.SocieteId.Equals(societeId)
                , cancellationToken
            )
            .ConfigureAwait(false);
    }
}