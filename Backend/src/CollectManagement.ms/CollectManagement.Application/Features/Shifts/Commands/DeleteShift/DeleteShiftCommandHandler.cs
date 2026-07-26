using CollectManagement.Application.Interfaces.Repositories.Shifts;
using CollectManagement.Domain.Shifts.ValueObjects;

namespace CollectManagement.Application.Features.Shifts.Commands.DeleteShift;

public class DeleteShiftCommandHandler
    : IRequestHandler<DeleteShiftCommand>
{
    private readonly IShiftRepository _shiftRepository;

    public DeleteShiftCommandHandler(IShiftRepository shiftRepository)
    {
        _shiftRepository = shiftRepository;
    }

    public async Task Handle(DeleteShiftCommand request, CancellationToken cancellationToken)
    {
        var shiftId = new ShiftId(request.ShiftId);

        await _shiftRepository
            .DeleteAsync(
                w => w.ShiftId.Equals(shiftId),
                cancellationToken
            )
            .ConfigureAwait(false);
    }
}
