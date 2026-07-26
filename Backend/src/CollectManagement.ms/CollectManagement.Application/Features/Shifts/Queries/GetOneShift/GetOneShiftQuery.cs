namespace CollectManagement.Application.Features.Shifts.Queries.GetOneShift;

public record GetOneShiftQuery(Ulid ShiftId) : IRequest<GetOneShiftResponse>;
