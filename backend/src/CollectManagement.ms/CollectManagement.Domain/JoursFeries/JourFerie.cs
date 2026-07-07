using CollectManagement.Domain.Common;
using CollectManagement.Domain.JoursFeries.ValueObjects;

namespace CollectManagement.Domain.JoursFeries;

public class JourFerie : AuditableEntity
{
    public JourFerieId JourFerieId { get; private set; }

    public DateTime Date { get; private set; }

    public string Label { get; private set; }

    private JourFerie(
        JourFerieId jourFerieId,
        DateTime date,
        string label)
    {
        JourFerieId = jourFerieId;
        Date = date;
        Label = label;
    }

    public static JourFerie Create(
        JourFerieId jourFerieId,
        DateTime date,
        string label)
    {
        return new JourFerie(jourFerieId, date.Date, label);
    }

    public void Update(
        DateTime date,
        string label)
    {
        Date = date.Date;
        Label = label;
    }

#pragma warning disable CS8618
    private JourFerie() { }
#pragma warning restore CS8618
}
