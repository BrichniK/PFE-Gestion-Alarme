using CollectManagement.Domain.Common;
using CollectManagement.Domain.Types.ValueObjects;

namespace CollectManagement.Domain.Types;

public class Type : AuditableEntity
{
    public TypeId TypeId { get; private set; }
    
    public string Code { get; private set; }
    
    public string Label { get; private set; }
    
    public int? DureeNominal { get; private set; }
    
    private Type(
        TypeId typeId,
        string code,
        string label,
        int? dureeNominal
        )
    {
        TypeId = typeId;
        Code = code;
        Label = label;
        DureeNominal = dureeNominal;
    }
    
    public static Type Create(
        TypeId typeId,
        string code,
        string label,
        int? dureeNominal)
    {
        return new Type(
            typeId,
            code,
            label,
            dureeNominal);
    }
    
    public void Update(
        string code,
        string label,
        int? dureeNominal
        )
    {
        Code = code;
        Label = label;
        DureeNominal = dureeNominal;
    }
    
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private Type() { }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    
}