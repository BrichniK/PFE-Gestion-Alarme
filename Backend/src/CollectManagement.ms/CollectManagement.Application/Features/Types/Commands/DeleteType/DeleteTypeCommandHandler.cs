using CollectManagement.Application.Interfaces.Repositories.Types;
using CollectManagement.Domain.Types.ValueObjects;

namespace CollectManagement.Application.Features.Types.Commands.DeleteType;

public class DeleteTypeCommandHandler
    : IRequestHandler<DeleteTypeCommand>
{
    private readonly ITypeRepository _typeRepository;

    public DeleteTypeCommandHandler(ITypeRepository typeRepository)
    {
        _typeRepository = typeRepository;
    }

    public async Task Handle(DeleteTypeCommand request, CancellationToken cancellationToken)
    {
        var typeId = new TypeId(request.TypeId);

        await _typeRepository
            .DeleteAsync(
                w => w.TypeId.Equals(typeId),
                cancellationToken
            )
            .ConfigureAwait(false);
    }
}
