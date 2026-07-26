using CollectManagement.Application.Exceptions;
using CollectManagement.Application.Interfaces.Groupes;
using CollectManagement.Domain.Groupes.ValueObjects;

namespace CollectManagement.Application.Features.Groupes.Queries.GetOneGroupe;

public class GetOneGroupeQueryHandler
    : IRequestHandler<GetOneGroupeQuery, GetOneGroupeResponse>
{
    private readonly IGroupeRepository _groupeRepository;
    private readonly IMapper _mapper;

    public GetOneGroupeQueryHandler(IGroupeRepository groupeRepository, IMapper mapper)
    {
        _groupeRepository = groupeRepository;
        _mapper = mapper;
    }

    public async Task<GetOneGroupeResponse> Handle(GetOneGroupeQuery request, CancellationToken cancellationToken)
    {
        var groupeId = new GroupeId(request.GroupeId);

        var groupe = await _groupeRepository
            .GetOneAsync(groupeId, cancellationToken)
            .ConfigureAwait(false) ?? throw new NotFoundException("Groupe NotFound.");

        return _mapper.Map<GetOneGroupeResponse>(groupe);
    }
}
