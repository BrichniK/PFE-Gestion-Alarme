using CollectManagement.Application.Interfaces.Repositories.Types;
using CollectManagement.Domain.Types.ValueObjects;
using Type = CollectManagement.Domain.Types.Type;

namespace CollectManagement.Application.Features.Types.Commands.CreateType;

public class CreateTypeCommandHandler
    : IRequestHandler<CreateTypeCommand, CreateTypeResponse>
{
    private readonly ITypeRepository _typeRepository;
    private readonly IMapper _mapper;

    public CreateTypeCommandHandler(
        ITypeRepository typeRepository,
        IMapper mapper)
    {
        _typeRepository = typeRepository;
        _mapper = mapper;
    }

    public async Task<CreateTypeResponse> Handle(CreateTypeCommand request, CancellationToken cancellationToken)
    {
        var typeId = new TypeId(Ulid.NewUlid());

        var type = Type.Create(
            typeId,
            request.Code,
            request.Label,
            request.DureeNominal
        );

        await _typeRepository
            .AddAsync(type, cancellationToken)
            .ConfigureAwait(false);

        return _mapper.Map<CreateTypeResponse>(type);
    }
}
