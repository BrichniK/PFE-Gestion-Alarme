using CollectManagement.Application.Features.Societes.Commands.CreateSociete;
using CollectManagement.Application.Features.Societes.Mapping;
using CollectManagement.Application.Features.Societes.Queries.GetOneSociete;
using CollectManagement.Application.Features.Societes.Queries.GetPagedListSociete;
using CollectManagementDomain.Societes;
using CollectManagementDomain.Societes.ValueObjects;
using FluentAssertions;
using Mapster;

namespace CollectManagement.Tests.Features.Societes.Mapping;


public class SocieteMappingTests
{

    private readonly TypeAdapterConfig _config;


    public SocieteMappingTests()
    {
        _config = new TypeAdapterConfig();

        var mapping = new SocieteMapping();

        mapping.Register(_config);
    }



    private Societe CreateSociete()
    {

        return Societe.Create(
            new SocieteId(Ulid.NewUlid()),

            "logo.png",

            "Societe Test",

            "MF123456",

            "RNE123",

            50000,

            new DateTime(2025,1,1),

            "71111111",

            "72222222",

            "33111111",

            "33222222",

            "test@societe.com",

            "Tunis",

            "SOC001"
        );

    }





    [Fact]
    public void Should_Map_Societe_To_CreateSocieteResponse()
    {

        // Arrange

        var societe = CreateSociete();



        // Act

        var result =
            societe.Adapt<CreateSocieteReponse>(_config);



        // Assert


        result.Should().NotBeNull();

        result.SocieteId
            .Should()
            .Be(societe.SocieteId.Value);

    }





    [Fact]
    public void Should_Map_Societe_To_GetPagedListSocieteDto()
    {

        // Arrange

        var societe = CreateSociete();



        // Act

        var result =
            societe.Adapt<GetPagedListSocieteDto>(_config);



        // Assert


        result.Should().NotBeNull();

        result.Nom
            .Should()
            .Be("Societe Test");


        result.CodeSociete
            .Should()
            .Be("SOC001");


        result.Email
            .Should()
            .Be("test@societe.com");

    }





    [Fact]
    public void Should_Map_Societe_To_GetOneSocieteResponse()
    {

        // Arrange

        var societe = CreateSociete();



        // Act

        var result =
            societe.Adapt<GetOneSocieteResponse>(_config);



        // Assert


        result.Should().NotBeNull();


        result.Nom
            .Should()
            .Be("Societe Test");


        result.MatriculeFiscal
            .Should()
            .Be("MF123456");


        result.Adresse
            .Should()
            .Be("Tunis");

    }

}