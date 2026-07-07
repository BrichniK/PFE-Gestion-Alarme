using CollectManagement.Domain.Common;
using CollectManagement.Domain.Utilisateurs.ValueObjects;

namespace CollectManagement.Domain.Utilisateurs.Entities;

public class RoleUtilisateur : AuditableEntity
{
    private readonly List<Navigation> _navigations = [];
    
    public RoleUtilisateurId RoleUtilisateurId { get; private set; }
    public string LibelleRoleUtilisateur { get; private set; }
    
    public IEnumerable<Navigation> Navigations => _navigations;

    private RoleUtilisateur(RoleUtilisateurId roleUtilisateurId, string libelleRoleUtilisateur, List<Navigation> navigations)
    {
        RoleUtilisateurId = roleUtilisateurId;
        LibelleRoleUtilisateur = libelleRoleUtilisateur;
        _navigations = navigations;
    }

    public static RoleUtilisateur Create(RoleUtilisateurId roleUtilisateurId, string libelleRoleUtilisateur, List<Navigation> navigations)
    {
        return new RoleUtilisateur(roleUtilisateurId, libelleRoleUtilisateur, navigations);
    }

    public void Update(string libelleRoleUtilisateur, List<Navigation> navigations)
    {
        LibelleRoleUtilisateur = libelleRoleUtilisateur;
        _navigations.Clear();
        _navigations.AddRange(navigations);
    }
    
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private RoleUtilisateur(){}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
}