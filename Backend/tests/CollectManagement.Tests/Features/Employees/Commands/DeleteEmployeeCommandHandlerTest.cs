using CollectManagement.Application.Features.Employees.Commands.DeleteEmployee;
using CollectManagement.Application.Interfaces.Employees;
using Moq;
using Xunit;

namespace CollectManagement.Tests.Features.Employees.Commands;

public class DeleteEmployeeCommandHandlerTests 
{
    [Fact]
    public async Task Handle_Should_Delete_Societe()
    {

        var repo = new Mock<IEmployeeRepository>();


        var handler =
            new DeleteEmployeeCommandHandler(repo.Object);


        var command = new DeleteEmployeeCommand(
            Ulid.NewUlid()
        );


        await handler.Handle(
            command,
            CancellationToken.None
        );


        repo.Verify(
            x=>x.DeleteAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<CollectManagement.Domain.Employess.Employee,bool>>>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );

    }

}