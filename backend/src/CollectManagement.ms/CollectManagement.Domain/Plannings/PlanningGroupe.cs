using CollectManagement.Domain.Common;
using CollectManagement.Domain.Groupes;
using CollectManagement.Domain.Groupes.ValueObjects;
using CollectManagement.Domain.Plannings.ValueObjects;

namespace CollectManagement.Domain.Plannings;

public class PlanningGroupe : AuditableEntity
{
    public PlanningId PlanningId { get; private set; }
    public Planning Planning { get; private set; }
    public GroupeId GroupeId { get; private set; }
    public Groupe Groupe { get; private set; }

    private PlanningGroupe(
        PlanningId planningId,
        GroupeId groupeId)
    {
        PlanningId = planningId;
        GroupeId = groupeId;
    }

    public static PlanningGroupe Create(
        PlanningId planningId,
        GroupeId groupeId)
    {
        return new PlanningGroupe(planningId, groupeId);
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private PlanningGroupe() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
