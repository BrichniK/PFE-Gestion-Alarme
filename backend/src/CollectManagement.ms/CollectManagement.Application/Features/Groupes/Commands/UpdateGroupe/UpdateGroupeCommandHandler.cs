using CollectManagement.Application.Interfaces.Groupes;
using CollectManagement.Domain.Groupes;
using CollectManagement.Domain.Groupes.ValueObjects;

namespace CollectManagement.Application.Features.Groupes.Commands.UpdateGroupe;

public class UpdateGroupeCommandHandler
    : IRequestHandler<UpdateGroupeCommand>
{
    private readonly IGroupeRepository _groupeRepository;

    public UpdateGroupeCommandHandler(IGroupeRepository groupeRepository)
    {
        _groupeRepository = groupeRepository;
    }

    public async Task Handle(UpdateGroupeCommand request, CancellationToken cancellationToken)
    {
        var groupeId = new GroupeId(request.GroupeId);

        var groupe = Groupe.Create(
            groupeId,
            request.Nom,
            request.Color,
            request.EmployeeIds
        );

        await _groupeRepository.UpdateBulkAsync(groupe, cancellationToken)
            .ConfigureAwait(false);
    }
}
