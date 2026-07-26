using CollectManagement.Application.Interfaces.Groupes;
using CollectManagement.Domain.Groupes;
using CollectManagement.Domain.Groupes.ValueObjects;

namespace CollectManagement.Application.Features.Groupes.Commands.CreateGroupe;

public class CreateGroupeCommandHandler
    : IRequestHandler<CreateGroupeCommand, CreateGroupeResponse>
{
    private readonly IGroupeRepository _groupeRepository;
    private readonly IMapper _mapper;

    public CreateGroupeCommandHandler(
        IGroupeRepository groupeRepository,
        IMapper mapper)
    {
        _groupeRepository = groupeRepository;
        _mapper = mapper;
    }

    public async Task<CreateGroupeResponse> Handle(CreateGroupeCommand request, CancellationToken cancellationToken)
    {
        var groupeId = new GroupeId(Ulid.NewUlid());

        var groupe = Groupe.Create(
            groupeId,
            request.Nom,
            request.Color,
            request.EmployeeIds
        );

        await _groupeRepository
            .AddAsync(groupe, cancellationToken)
            .ConfigureAwait(false);

        return _mapper.Map<CreateGroupeResponse>(groupe);
    }
}
