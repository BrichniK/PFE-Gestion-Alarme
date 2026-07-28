using FluentAssertions;
using Moq;
using CollectManagement.Application.Features.Societes.Queries.GetOneSociete;
using CollectManagement.Application.Interfaces.Societes;
using CollectManagementDomain.Societes;


namespace CollectManagement.Tests.Features.Societes.Queries;


public class GetOneSocieteQueryHandlerTests
{


    [Fact]
    public async Task Handle_Should_Return_Societe()
    {


        var repo = new Mock<ISocieteRepository>();
        var mapper = new Mock<MapsterMapper.IMapper>();


        var societe =
            Societe.Create(
                new(Ulid.NewUlid()),
                null,
                "TEST",
                "MF",
                "RNE",
                1000,
                DateTime.Now,
                null,
                null,
                null,
                null,
                "test@test.com",
                null,
                "CODE"
            );


        repo.Setup(x=>x.GetOneAsync(
                It.IsAny<CollectManagementDomain.Societes.ValueObjects.SocieteId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(societe);



        mapper.Setup(x => x.Map<GetOneSocieteResponse>(
                It.IsAny<Societe>()))
            .Returns(new GetOneSocieteResponse(
                societe.SocieteId.Value,
                societe.LogoPath,
                societe.Nom,
                societe.LogoPath,
                societe.Rne,
                societe.MatriculeFiscal,
                societe.Rne,
                societe.Capital,
                societe.DateOverture,
                societe.Telephone1,
                societe.Telephone2,
                societe.Fax1,
                societe.Fax2,
                societe.Email,
                null,
                null
            ));


        var handler =
            new GetOneSocieteQueryHandler(
                repo.Object,
                mapper.Object
            );


        var result =
            await handler.Handle(
                new GetOneSocieteQuery(
                    societe.SocieteId.Value
                ),
                CancellationToken.None
            );


        result.Should().NotBeNull();

    }

}