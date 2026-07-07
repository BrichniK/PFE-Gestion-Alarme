using CollectManagement.Application.Features.JoursFeries.Commands.CreateJourFerie;
using CollectManagement.Application.Features.JoursFeries.Queries.GetOneJourFerie;
using CollectManagement.Application.Features.JoursFeries.Queries.GetPagedListJourFerie;
using CollectManagement.Domain.JoursFeries;

namespace CollectManagement.Application.Features.JoursFeries.Mapping;

public class JourFerieMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<JourFerie, CreateJourFerieResponse>()
            .Map(d => d.JourFerieId, s => s.JourFerieId.Value);

        config.NewConfig<JourFerie, GetPagedListJourFerieDto>()
            .Map(d => d.JourFerieId, s => s.JourFerieId.Value)
            .Map(d => d.Date, s => s.Date)
            .Map(d => d.Label, s => s.Label);

        config.NewConfig<JourFerie, GetOneJourFerieResponse>()
            .Map(d => d.JourFerieId, s => s.JourFerieId.Value)
            .Map(d => d.Date, s => s.Date)
            .Map(d => d.Label, s => s.Label);
    }
}
