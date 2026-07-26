using CollectManagement.Domain.Utilisateurs.Enums;

namespace CollectManagement.Domain.Utilisateurs.Entities;

public class NavigationSection
{
    private readonly List<NavigationAction> _actions = [];

    public string SectionId { get; private set; }

    public IEnumerable<NavigationAction> Actions => _actions;

    private NavigationSection(string sectionId, List<NavigationAction> actions)
    {
        SectionId = sectionId;
        _actions = actions;
    }

    public static NavigationSection Create(string sectionId, List<NavigationAction> actions)
    {
        return new NavigationSection( sectionId, actions);
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private NavigationSection() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}