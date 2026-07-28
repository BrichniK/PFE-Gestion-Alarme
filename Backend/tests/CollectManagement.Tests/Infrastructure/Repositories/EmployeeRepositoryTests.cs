using CollectManagement.Application.Interfaces.Employees;
using CollectManagement.Domain.Employess;
using CollectManagement.Domain.Employess.ObjectValues;
using FluentAssertions;
using Moq;


namespace CollectManagement.Tests.Infrastructure.Repositories;


public class EmployeeRepositoryTests
{

    private readonly Mock<IEmployeeRepository> _repository;


    public EmployeeRepositoryTests()
    {
        _repository = new Mock<IEmployeeRepository>();
    }



    [Fact]
    public async Task GetOneAsync_Should_Return_Employee()
    {

        var id =
            new EmployeeId(Ulid.NewUlid());



        var employee =
            Employee.Create(
                id,
                "Khalil",
                "Brichni",
                71111111,
                "RFID001",
                "test@test.com",
                null);



        _repository
            .Setup(x=>x.GetOneAsync(
                id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);



        var result =
            await _repository.Object.GetOneAsync(
                id,
                CancellationToken.None);



        result.Should()
            .NotBeNull();


        result.EmployeeId
            .Should()
            .Be(id);



        _repository.Verify(
            x=>x.GetOneAsync(
                id,
                It.IsAny<CancellationToken>()),
            Times.Once);

    }

}