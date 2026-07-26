namespace CollectManagement.Domain.Common;

public class AuditableEntity
{
    public string? InsererPar { get; set; }
    public DateTime? DateInsertion { get; set; }
    public string? ModifierPar { get; set; }
    public DateTime? DateModification { get; set; }
}