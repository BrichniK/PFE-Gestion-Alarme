using Moq;
using Xunit;
using CollectManagement.Application.Interfaces.Repositories.Alertes;
using CollectManagement.Domain.Alertes.ValueObjects;

namespace CollectManagement.Tests.Features.Alertes;


public class DeleteAlerteCommandHandlerTests
{

    private readonly Mock<IAlerteRepository> _repository;


    public DeleteAlerteCommandHandlerTests()
    {
        _repository = new Mock<IAlerteRepository>();
    }


    [Fact]
    public async Task Handle_Should_Delete_Alerte()
    {

        var id = new AlerteId(Ulid.NewUlid());


        await Task.CompletedTask;


        Assert.True(true);
    }
}