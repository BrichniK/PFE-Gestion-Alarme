using CollectManagement.Application.Features.Maintenances.Commands.CreateMaintenance;
using CollectManagement.Application.Features.Maintenances.Queries.GetOneMaintenance;
using CollectManagement.Application.Features.Maintenances.Queries.GetPagedListMaintenance;
using CollectManagement.Domain.Maintenances;

namespace CollectManagement.Application.Features.Maintenances.Mapping;

public class MaintenanceMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Maintenance, CreateMaintenanceResponse>()
            .Map(d => d.MaintenanceId, s => s.MaintenanceId.Value);

        config.NewConfig<Maintenance, GetPagedListMaintenanceDto>()
            .Map(d => d.MaintenanceId, s => s.MaintenanceId.Value)
            .Map(d => d.DeviceId, s => s.DeviceId.Value)
            .Map(d => d.DeviceName, s => s.Device != null ? s.Device.DeviceName : null)
            .Map(d => d.EmployeeId, s => s.EmployeeId.Value)
            .Map(d => d.EmployeeNom, s => s.Employee != null ? s.Employee.Nom : null)
            .Map(d => d.EmployeePrenom, s => s.Employee != null ? s.Employee.Prenom : null)
            .Map(d => d.T1Alerte, s => s.T1Alerte)
            .Map(d => d.T2Assignment, s => s.T2Assignment)
            .Map(d => d.T3Arrival, s => s.T3Arrival)
            .Map(d => d.T4Completion, s => s.T4Completion)
            .Map(d => d.T5Confirmation, s => s.T5Confirmation)
            .Map(d => d.T6NextAlert, s => s.T6NextAlert)
            .Map(d => d.Description, s => s.Description);

        config.NewConfig<Maintenance, GetOneMaintenanceResponse>()
            .Map(d => d.MaintenanceId, s => s.MaintenanceId.Value)
            .Map(d => d.DeviceId, s => s.DeviceId.Value)
            .Map(d => d.DeviceName, s => s.Device != null ? s.Device.DeviceName : null)
            .Map(d => d.EmployeeId, s => s.EmployeeId.Value)
            .Map(d => d.EmployeeNom, s => s.Employee != null ? s.Employee.Nom : null)
            .Map(d => d.EmployeePrenom, s => s.Employee != null ? s.Employee.Prenom : null)
            .Map(d => d.T1Alerte, s => s.T1Alerte)
            .Map(d => d.T2Assignment, s => s.T2Assignment)
            .Map(d => d.T3Arrival, s => s.T3Arrival)
            .Map(d => d.T4Completion, s => s.T4Completion)
            .Map(d => d.T5Confirmation, s => s.T5Confirmation)
            .Map(d => d.T6NextAlert, s => s.T6NextAlert)
            .Map(d => d.Description, s => s.Description);
    }
}
