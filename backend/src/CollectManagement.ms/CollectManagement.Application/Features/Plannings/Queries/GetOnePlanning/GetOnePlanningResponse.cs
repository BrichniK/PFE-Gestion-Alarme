namespace CollectManagement.Application.Features.Plannings.Queries.GetOnePlanning;

public record GetOnePlanningResponse(
    Ulid PlanningId,
    DateTime Date,
    string AssignmentMode,
    List<Ulid> GroupeIds,
    List<string> GroupeColors,
    List<Ulid> DeviceIds,
    List<Ulid> ShiftIds,
    List<Ulid> EmployeeIds,
    Ulid GroupeId,
    string? GroupeNom,
    Ulid DeviceId,
    string? DeviceName,
    Ulid ShiftId,
    string? ShiftLabel
);
