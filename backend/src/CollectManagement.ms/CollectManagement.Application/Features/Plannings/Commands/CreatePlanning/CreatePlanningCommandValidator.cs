using FluentValidation;

namespace CollectManagement.Application.Features.Plannings.Commands.CreatePlanning;

public class CreatePlanningCommandValidator
    : AbstractValidator<CreatePlanningCommand>
{
    public CreatePlanningCommandValidator()
    {
        RuleFor(r => r)
            .Must(r => HasValidDates(r))
            .WithMessage("Date or Dates is required.");

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

    private static bool HasValidDates(CreatePlanningCommand request)
    {
        if (request.Dates is { Count: > 0 })
        {
            return request.Dates.All(date => date != default);
        }

        return request.Date != default;
    }
}
