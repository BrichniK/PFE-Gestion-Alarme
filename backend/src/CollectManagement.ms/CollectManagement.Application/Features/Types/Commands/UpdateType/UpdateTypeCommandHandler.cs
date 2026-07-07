using CollectManagement.Application.Interfaces.Repositories.Types;
using CollectManagement.Domain.Types.ValueObjects;
using Type = CollectManagement.Domain.Types.Type;

namespace CollectManagement.Application.Features.Types.Commands.UpdateType;

public class UpdateTypeCommandHandler
    : IRequestHandler<UpdateTypeCommand>
{
    private readonly ITypeRepository _typeRepository;

    public UpdateTypeCommandHandler(ITypeRepository typeRepository)
    {
        _typeRepository = typeRepository;
    }

    public async Task Handle(UpdateTypeCommand request, CancellationToken cancellationToken)
    {
        var typeId = new TypeId(request.TypeId);

        var type = Type.Create(
            typeId,
            request.Code,
            request.Label,
            request.DureeNominal
        );

        await _typeRepository.UpdateBulkAsync(type, cancellationToken)
            .ConfigureAwait(false);
    }
}
