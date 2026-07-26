using CollectManagement.Application.Features.Groupes.Commands.CreateGroupe;
using CollectManagement.Application.Features.Groupes.Queries.GetOneGroupe;
using CollectManagement.Application.Features.Groupes.Queries.GetPagedListGroupe;
using CollectManagement.Domain.Groupes;

namespace CollectManagement.Application.Features.Groupes.Mapping;

public class GroupeMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Groupe, CreateGroupeResponse>()
            .Map(d => d.GroupeId, s => s.GroupeId.Value);

        config.NewConfig<Groupe, GetPagedListGroupeDto>()
            .Map(d => d.GroupeId, s => s.GroupeId.Value)
            .Map(d => d.Nom, s => s.Nom)
            .Map(d => d.Color, s => s.Color)
            .Map(d => d.EmployeeIds, s => s.EmployeeIds);

        config.NewConfig<Groupe, GetOneGroupeResponse>()
            .Map(d => d.GroupeId, s => s.GroupeId.Value)
            .Map(d => d.Nom, s => s.Nom)
            .Map(d => d.Color, s => s.Color)
            .Map(d => d.EmployeeIds, s => s.EmployeeIds);
    }
}
