using CollectManagement.Application.Features.Employees.Queries.GetOneEmployee;
using CollectManagement.Application.Interfaces.Employees;
using CollectManagement.Domain.Employess;
using FluentAssertions;
using MapsterMapper;
using Moq;

namespace CollectManagement.Tests.Features.Employees.Queries;


public class GetOneEmployeeQueryHandlerTests
{

    [Fact]
    public async Task Handle_Should_Return_Employee()
    {

        var repository =
            new Mock<IEmployeeRepository>();

        var mapper =
            new Mock<IMapper>();


        var employee =
            Employee.Create(
                new CollectManagement.Domain.Employess.ObjectValues.EmployeeId(Ulid.NewUlid()),
                "K",
                "B",
                71111111,
                "RFID",
                null,
                null);



        repository
            .Setup(x=>x.GetOneAsync(
                It.IsAny<
                    CollectManagement.Domain.Employess.ObjectValues.EmployeeId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);



        mapper
            .Setup(x=>x.Map<GetOneEmployeeResponse>(
                employee))
            .Returns(
                new GetOneEmployeeResponse(
                    employee.EmployeeId.Value,
                    "K",
                    "B",
                    71111111,
                    "RFID",
                    null,
                    null));



        var handler =
            new GetOneEmployeeQueryHandler(
                repository.Object,
                mapper.Object);



        var result =
            await handler.Handle(
                new GetOneEmployeeQuery(
                    employee.EmployeeId.Value),
                CancellationToken.None);



        result.Should().NotBeNull();

    }

}