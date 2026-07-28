using CollectManagement.Application.Features.Employees.Commands.UpdateEmployee;
using CollectManagement.Application.Interfaces.Employees;
using CollectManagement.Application.Interfaces.Services;
using CollectManagement.Domain.Employess;
using Moq;

namespace CollectManagement.Tests.Features.Employees.Commands;


public class UpdateEmployeeCommandHandlerTests
{


    [Fact]
    public async Task Handle_Should_Update_Employee()
    {

        var repository =
            new Mock<IEmployeeRepository>();

        var image =
            new Mock<IImageService>();



        repository
            .Setup(x=>x.UpdateBulkAsync(
                It.IsAny<Employee>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);



        var handler =
            new UpdateEmployeeCommandHandler(
                repository.Object,
                image.Object);



        var command =
            new UpdateEmployeeCommand(
                Ulid.NewUlid(),
                null,
                null,
                55555,
                "Nom",
                "Prenom",
                null,
                "RFID",
                "test@test.com"
            );



        await handler.Handle(
            command,
            CancellationToken.None);



        repository.Verify(
            x=>x.UpdateBulkAsync(
                It.IsAny<Employee>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

    }

}