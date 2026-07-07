using CollectManagement.Domain.Utilisateurs.Enums;
using CollectManagement.Domain.Utilisateurs.ValueObjects;

namespace CollectManagement.Domain.Utilisateurs.Entities;

public class Navigation
{
    private readonly List<NavigationAction> _actions = [];
    private readonly List<NavigationSection> _sections = [];
    public string NavigationId { get; private set; }
    public RoleUtilisateurId RoleUtilisateurId { get; private set; }
    public IEnumerable<NavigationAction> Actions => _actions;
    public IEnumerable<NavigationSection> Sections => _sections;

    private Navigation(string navigationId, RoleUtilisateurId roleUtilisateurId, List<NavigationAction> actions, List<NavigationSection> sections)
    {
        NavigationId = navigationId;
        RoleUtilisateurId = roleUtilisateurId;
        _actions = actions;
        _sections = sections;
    }

    public static Navigation Create(string navigationId, RoleUtilisateurId roleUtilisateurId, List<NavigationAction> actions, List<NavigationSection> sections)
    {
        return new Navigation(navigationId, roleUtilisateurId, actions, sections);
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private Navigation() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}