namespace CollectManagement.Application.Features.Shifts.Queries.GetPagedListShift;

public record GetPagedListShiftResponse(
    List<GetPagedListShiftDto> Shifts,
    int Length
);
