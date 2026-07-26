using CollectManagement.Application.Exceptions;
using CollectManagement.Application.Interfaces.Repositories.Shifts;
using CollectManagement.Domain.Shifts.ValueObjects;

namespace CollectManagement.Application.Features.Shifts.Queries.GetOneShift;

public class GetOneShiftQueryHandler
    : IRequestHandler<GetOneShiftQuery, GetOneShiftResponse>
{
    private readonly IShiftRepository _shiftRepository;
    private readonly IMapper _mapper;

    public GetOneShiftQueryHandler(IShiftRepository shiftRepository, IMapper mapper)
    {
        _shiftRepository = shiftRepository;
        _mapper = mapper;
    }

    public async Task<GetOneShiftResponse> Handle(GetOneShiftQuery request, CancellationToken cancellationToken)
    {
        var shiftId = new ShiftId(request.ShiftId);

        var shift = await _shiftRepository
            .GetOneAsync(shiftId, cancellationToken)
            .ConfigureAwait(false) ?? throw new NotFoundException("Shift NotFound.");

        return _mapper.Map<GetOneShiftResponse>(shift);
    }
}
