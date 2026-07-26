using CollectManagement.Domain.Common;
using CollectManagement.Domain.Shifts.ValueObjects;

namespace CollectManagement.Domain.Shifts;

public class Shift : AuditableEntity
{
    public ShiftId ShiftId { get; private set; }
    
    public string Label { get; private set; }
    
    public TimeOnly StartTime { get; private set; }
    
    public TimeOnly EndTime { get; private set; }
    
    private Shift(
        ShiftId shiftId,
        string label,
        TimeOnly startTime,
        TimeOnly endTime)
    {
        ShiftId = shiftId;
        Label = label;
        StartTime = startTime;
        EndTime = endTime;
    }
    
    public static Shift Create(
        ShiftId shiftId,
        string label,
        TimeOnly startTime,
        TimeOnly endTime)
    {
        // Add any necessary validation logic here
        
        return new Shift(shiftId, label, startTime, endTime);
    }
    
    
    public void Update(
        string label,
        TimeOnly startTime,
        TimeOnly endTime)
    {
        // Add any necessary validation logic here
        
        Label = label;
        StartTime = startTime;
        EndTime = endTime;
    }
    
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private Shift() { }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}