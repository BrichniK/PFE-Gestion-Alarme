using CollectManagement.Application.Interfaces.Groupes;
using CollectManagement.Domain.Groupes.ValueObjects;

namespace CollectManagement.Application.Features.Groupes.Commands.DeleteGroupe;

public class DeleteGroupeCommandHandler
    : IRequestHandler<DeleteGroupeCommand>
{
    private readonly IGroupeRepository _groupeRepository;

    public DeleteGroupeCommandHandler(IGroupeRepository groupeRepository)
    {
        _groupeRepository = groupeRepository;
    }

    public async Task Handle(DeleteGroupeCommand request, CancellationToken cancellationToken)
    {
        var groupeId = new GroupeId(request.GroupeId);

        await _groupeRepository
            .DeleteAsync(
                w => w.GroupeId.Equals(groupeId),
                cancellationToken
            )
            .ConfigureAwait(false);
    }
}
