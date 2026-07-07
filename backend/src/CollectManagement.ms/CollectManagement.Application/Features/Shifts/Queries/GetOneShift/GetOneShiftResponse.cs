namespace CollectManagement.Application.Features.Shifts.Queries.GetOneShift;

public record GetOneShiftResponse(
    Ulid ShiftId,
    string Label,
    TimeOnly StartTime,
    TimeOnly EndTime
);
