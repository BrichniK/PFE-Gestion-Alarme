using FluentValidation;

namespace CollectManagement.Application.Features.Plannings.Commands.UpdatePlanning;

public class UpdatePlanningCommandValidator
    : AbstractValidator<UpdatePlanningCommand>
{
    public UpdatePlanningCommandValidator()
    {
        RuleFor(r => r.PlanningId)
            .NotEmpty()
            .WithMessage("PlanningId is required.");

        RuleFor(r => r.Date)
            .NotEmpty()
            .WithMessage("Date is required.");

        RuleFor(r => NormalizeIds(r.GroupeIds, r.GroupeId))
            .Must(ids => ids.Count > 0 && ids.All(x => x != Ulid.Empty))
            .WithMessage("At least one valid GroupeId is required.");

        RuleFor(r => NormalizeIds(r.DeviceIds, r.DeviceId))
            .Must(ids => ids.Count > 0 && ids.All(x => x != Ulid.Empty))
            .WithMessage("At least one valid DeviceId is required.");

        RuleFor(r => NormalizeIds(r.ShiftIds, r.ShiftId))
            .Must(ids => ids.Count > 0 && ids.All(x => x != Ulid.Empty))
            .WithMessage("At least one valid ShiftId is required.");
    }

    private static IReadOnlyList<Ulid> NormalizeIds(IReadOnlyList<Ulid>? ids, Ulid singleId)
    {
        if (ids is { Count: > 0 })
            return ids;

        return singleId == Ulid.Empty
            ? Array.Empty<Ulid>()
            : new[] { singleId };
    }
}
