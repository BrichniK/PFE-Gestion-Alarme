using CollectManagement.Application.Features.Plannings.Commands.CreatePlanning;
using CollectManagement.Application.Features.Plannings.Queries.GetOnePlanning;
using CollectManagement.Application.Features.Plannings.Queries.GetPagedListPlanning;
using CollectManagement.Domain.Plannings;

namespace CollectManagement.Application.Features.Plannings.Mapping;

public class PlanningMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Planning, CreatePlanningResponse>()
            .Map(d => d.PlanningId, s => s.PlanningId.Value)
            .Map(d => d.PlanningIds, s => new[] { s.PlanningId.Value });

        config.NewConfig<Planning, GetPagedListPlanningDto>()
            .Map(d => d.PlanningId, s => s.PlanningId.Value)
            .Map(d => d.Date, s => s.Date)
            .Map(d => d.AssignmentMode, s => s.PlanningEmployees.Any() ? "employee" : "group")
            .Map(d => d.GroupeIds, s => s.PlanningGroupes.Select(pg => pg.GroupeId.Value).ToList())
            .Map(d => d.GroupeColors, s => s.PlanningGroupes.Select(pg => pg.Groupe.Color).ToList())
            .Map(d => d.DeviceIds, s => s.PlanningDevices.Select(pd => pd.DeviceId.Value).ToList())
            .Map(d => d.ShiftIds, s => s.PlanningShifts.Select(ps => ps.ShiftId.Value).ToList())
            .Map(d => d.EmployeeIds, s => s.PlanningEmployees.Select(pe => pe.EmployeeId.Value).ToList())
            .Map(d => d.GroupeId, s => s.PlanningGroupes.Select(pg => pg.GroupeId.Value).FirstOrDefault())
            .Map(d => d.GroupeNom, s => s.PlanningGroupes.Select(pg => pg.Groupe.Nom).FirstOrDefault())
            .Map(d => d.DeviceId, s => s.PlanningDevices.Select(pd => pd.DeviceId.Value).FirstOrDefault())
            .Map(d => d.DeviceName, s => s.PlanningDevices.Select(pd => pd.Device.DeviceName).FirstOrDefault())
            .Map(d => d.ShiftId, s => s.PlanningShifts.Select(ps => ps.ShiftId.Value).FirstOrDefault())
            .Map(d => d.ShiftLabel, s => s.PlanningShifts.Select(ps => ps.Shift.Label).FirstOrDefault());

        config.NewConfig<Planning, GetOnePlanningResponse>()
            .Map(d => d.PlanningId, s => s.PlanningId.Value)
            .Map(d => d.Date, s => s.Date)
            .Map(d => d.AssignmentMode, s => s.PlanningEmployees.Any() ? "employee" : "group")
            .Map(d => d.GroupeIds, s => s.PlanningGroupes.Select(pg => pg.GroupeId.Value).ToList())
            .Map(d => d.GroupeColors, s => s.PlanningGroupes.Select(pg => pg.Groupe.Color).ToList())
            .Map(d => d.DeviceIds, s => s.PlanningDevices.Select(pd => pd.DeviceId.Value).ToList())
            .Map(d => d.ShiftIds, s => s.PlanningShifts.Select(ps => ps.ShiftId.Value).ToList())
            .Map(d => d.EmployeeIds, s => s.PlanningEmployees.Select(pe => pe.EmployeeId.Value).ToList())
            .Map(d => d.GroupeId, s => s.PlanningGroupes.Select(pg => pg.GroupeId.Value).FirstOrDefault())
            .Map(d => d.GroupeNom, s => s.PlanningGroupes.Select(pg => pg.Groupe.Nom).FirstOrDefault())
            .Map(d => d.DeviceId, s => s.PlanningDevices.Select(pd => pd.DeviceId.Value).FirstOrDefault())
            .Map(d => d.DeviceName, s => s.PlanningDevices.Select(pd => pd.Device.DeviceName).FirstOrDefault())
            .Map(d => d.ShiftId, s => s.PlanningShifts.Select(ps => ps.ShiftId.Value).FirstOrDefault())
            .Map(d => d.ShiftLabel, s => s.PlanningShifts.Select(ps => ps.Shift.Label).FirstOrDefault());
    }
}
