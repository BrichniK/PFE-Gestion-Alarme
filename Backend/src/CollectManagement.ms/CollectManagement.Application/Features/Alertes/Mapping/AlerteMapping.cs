using CollectManagement.Application.Features.Alertes.Commands.CreateAlerte;
using CollectManagement.Application.Features.Alertes.Queries.GetOneAlerte;
using CollectManagement.Application.Features.Alertes.Queries.GetPagedListAlerte;
using CollectManagement.Domain.Alertes;

namespace CollectManagement.Application.Features.Alertes.Mapping;

public class AlerteMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Alerte, CreateAlerteResponse>()
            .Map(d => d.AlerteId, s => s.AlerteId.Value);

        config.NewConfig<Alerte, GetPagedListAlerteDto>()
            .Map(d => d.AlerteId, s => s.AlerteId.Value)
            .Map(d => d.Date, s => s.Date)
            .Map(d => d.DispositifId, s => s.DispositifId.Value)
            .Map(d => d.DispositifName, s => s.Dispositif.DeviceName)
            .Map(d => d.TypeId, s => s.TypeId.Value)
            .Map(d => d.Traiter, s => s.Traiter);

        config.NewConfig<Alerte, GetOneAlerteResponse>()
            .Map(d => d.AlerteId, s => s.AlerteId.Value)
            .Map(d => d.Date, s => s.Date)
            .Map(d => d.DispositifId, s => s.DispositifId.Value)
            .Map(d => d.TypeId, s => s.TypeId.Value)
            .Map(d => d.Traiter, s => s.Traiter);
    }
}
