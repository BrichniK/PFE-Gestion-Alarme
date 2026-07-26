using CollectManagement.Application.Features.Shifts.Commands.CreateShift;
using CollectManagement.Application.Features.Shifts.Queries.GetOneShift;
using CollectManagement.Application.Features.Shifts.Queries.GetPagedListShift;
using CollectManagement.Domain.Shifts;

namespace CollectManagement.Application.Features.Shifts.Mapping;

public class ShiftMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Shift, CreateShiftResponse>()
            .Map(d => d.ShiftId, s => s.ShiftId.Value);

        config.NewConfig<Shift, GetPagedListShiftDto>()
            .Map(d => d.ShiftId, s => s.ShiftId.Value)
            .Map(d => d.Label, s => s.Label)
            .Map(d => d.StartTime, s => s.StartTime)
            .Map(d => d.EndTime, s => s.EndTime);

        config.NewConfig<Shift, GetOneShiftResponse>()
            .Map(d => d.ShiftId, s => s.ShiftId.Value)
            .Map(d => d.Label, s => s.Label)
            .Map(d => d.StartTime, s => s.StartTime)
            .Map(d => d.EndTime, s => s.EndTime);
    }
}
