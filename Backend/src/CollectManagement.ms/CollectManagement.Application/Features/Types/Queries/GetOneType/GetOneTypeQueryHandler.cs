using CollectManagement.Application.Exceptions;
using CollectManagement.Application.Interfaces.Repositories.Types;
using CollectManagement.Domain.Types.ValueObjects;

namespace CollectManagement.Application.Features.Types.Queries.GetOneType;

public class GetOneTypeQueryHandler
    : IRequestHandler<GetOneTypeQuery, GetOneTypeResponse>
{
    private readonly ITypeRepository _typeRepository;
    private readonly IMapper _mapper;

    public GetOneTypeQueryHandler(ITypeRepository typeRepository, IMapper mapper)
    {
        _typeRepository = typeRepository;
        _mapper = mapper;
    }

    public async Task<GetOneTypeResponse> Handle(GetOneTypeQuery request, CancellationToken cancellationToken)
    {
        var typeId = new TypeId(request.TypeId);

        var type = await _typeRepository
            .GetOneAsync(typeId, cancellationToken)
            .ConfigureAwait(false) ?? throw new NotFoundException("Type NotFound.");

        return _mapper.Map<GetOneTypeResponse>(type);
    }
}
