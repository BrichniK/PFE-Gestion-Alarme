namespace CollectManagement.Application.Features.Shifts.Commands.CreateShift;

public record CreateShiftCommand(
    string Label,
    TimeOnly StartTime,
    TimeOnly EndTime
) : IRequest<CreateShiftResponse>;
