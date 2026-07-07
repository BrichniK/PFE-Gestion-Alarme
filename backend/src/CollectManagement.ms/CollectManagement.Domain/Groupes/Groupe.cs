using CollectManagement.Domain.Common;
using CollectManagement.Domain.Groupes.ValueObjects;

namespace CollectManagement.Domain.Groupes;

public class Groupe : AuditableEntity
{
    public GroupeId GroupeId { get; private set; }
    
    public string Nom { get; private set; }
    
    public string Color { get; private set; }
    
    public List<Ulid> EmployeeIds { get; private set; }

    private Groupe(
        GroupeId groupeId,
        string nom,
        string color,
        List<Ulid> employeeIds)
    {
        GroupeId = groupeId;
        Nom = nom;
        Color = color;
        EmployeeIds = employeeIds;
    }

    public static Groupe Create(
        GroupeId groupeId,
        string nom,
        string color,
        List<Ulid> employeeIds)
    {
        return new Groupe(
            groupeId,
            nom,
            color,
            employeeIds);
    }

#pragma warning disable CS8618
    private Groupe() { }
#pragma warning restore CS8618
}