using CollectManagement.Application.Interfaces.Repositories.Shifts;
using CollectManagement.Domain.Shifts;
using CollectManagement.Domain.Shifts.ValueObjects;

namespace CollectManagement.Application.Features.Shifts.Commands.UpdateShift;

public class UpdateShiftCommandHandler
    : IRequestHandler<UpdateShiftCommand>
{
    private readonly IShiftRepository _shiftRepository;

    public UpdateShiftCommandHandler(IShiftRepository shiftRepository)
    {
        _shiftRepository = shiftRepository;
    }

    public async Task Handle(UpdateShiftCommand request, CancellationToken cancellationToken)
    {
        var shiftId = new ShiftId(request.ShiftId);

        var shift = Shift.Create(
            shiftId,
            request.Label,
            request.StartTime,
            request.EndTime
        );

        await _shiftRepository.UpdateBulkAsync(shift, cancellationToken)
            .ConfigureAwait(false);
    }
}
