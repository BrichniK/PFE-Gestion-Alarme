namespace CollectManagement.Application.Features.Shifts.Queries.GetPagedListShift;

public record GetPagedListShiftDto(
    Ulid ShiftId,
    string Label,
    TimeOnly StartTime,
    TimeOnly EndTime
);
