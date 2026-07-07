using CollectManagement.Application.Interfaces.Repositories.Shifts;
using CollectManagement.Domain.Shifts;
using CollectManagement.Domain.Shifts.ValueObjects;

namespace CollectManagement.Application.Features.Shifts.Commands.CreateShift;

public class CreateShiftCommandHandler
    : IRequestHandler<CreateShiftCommand, CreateShiftResponse>
{
    private readonly IShiftRepository _shiftRepository;
    private readonly IMapper _mapper;

    public CreateShiftCommandHandler(
        IShiftRepository shiftRepository,
        IMapper mapper)
    {
        _shiftRepository = shiftRepository;
        _mapper = mapper;
    }

    public async Task<CreateShiftResponse> Handle(CreateShiftCommand request, CancellationToken cancellationToken)
    {
        var shiftId = new ShiftId(Ulid.NewUlid());

        var shift = Shift.Create(
            shiftId,
            request.Label,
            request.StartTime,
            request.EndTime
        );

        await _shiftRepository
            .AddAsync(shift, cancellationToken)
            .ConfigureAwait(false);

        return _mapper.Map<CreateShiftResponse>(shift);
    }
}
