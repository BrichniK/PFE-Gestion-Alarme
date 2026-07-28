using CollectManagement.Application.Features.Groupes.Commands.CreateGroupe;
using CollectManagement.Application.Features.Groupes.Mapping;
using CollectManagement.Domain.Groupes;
using CollectManagement.Domain.Groupes.ValueObjects;
using FluentAssertions;
using Mapster;

namespace CollectManagement.Tests.Features.Groupes.Mapping;


public class GroupeMappingTests
{

    [Fact]
    public void Groupe_Should_Map_To_CreateResponse()
    {

        var config =
            new TypeAdapterConfig();


        new GroupeMapping()
            .Register(config);



        var groupe =
            Groupe.Create(
                new GroupeId(Ulid.NewUlid()),
                "Equipe",
                "#FFF",
                new List<Ulid>());



        var result =
            groupe.Adapt<CreateGroupeResponse>(config);



        result.Should()
            .NotBeNull();


        result.GroupeId
            .Should()
            .Be(groupe.GroupeId.Value);

    }

}