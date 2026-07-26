namespace CollectManagement.Application.Features.Shifts.Commands.DeleteShift;

public record DeleteShiftCommand(Ulid ShiftId) : IRequest;
