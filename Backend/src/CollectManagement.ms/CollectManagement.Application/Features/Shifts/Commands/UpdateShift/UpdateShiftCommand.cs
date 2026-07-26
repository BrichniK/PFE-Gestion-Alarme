namespace CollectManagement.Application.Features.Shifts.Commands.UpdateShift;

public record UpdateShiftCommand(
    Ulid ShiftId,
    string Label,
    TimeOnly StartTime,
    TimeOnly EndTime
) : IRequest;
