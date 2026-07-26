using CollectManagement.Application.Features.Types.Commands.CreateType;
using CollectManagement.Application.Features.Types.Queries.GetOneType;
using CollectManagement.Application.Features.Types.Queries.GetPagedListType;
using Type = CollectManagement.Domain.Types.Type;

namespace CollectManagement.Application.Features.Types.Mapping;

public class TypeMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Type, CreateTypeResponse>()
            .Map(d => d.TypeId, s => s.TypeId.Value);

        config.NewConfig<Type, GetPagedListTypeDto>()
            .Map(d => d.TypeId, s => s.TypeId.Value)
            .Map(d => d.Code, s => s.Code)
            .Map(d => d.Label, s => s.Label)
            .Map(d => d.DureeNominal, s => s.DureeNominal);

        config.NewConfig<Type, GetOneTypeResponse>()
            .Map(d => d.TypeId, s => s.TypeId.Value)
            .Map(d => d.Code, s => s.Code)
            .Map(d => d.Label, s => s.Label)
            .Map(d => d.DureeNominal, s => s.DureeNominal);
    }
}
