using CollectManagement.Application.Features.Employees.Queries.GetPagedListEmployee;
using CollectManagement.Application.Interfaces.Employees;
using FluentAssertions;
using Moq;

namespace CollectManagement.Tests.Features.Employees.Queries;


public class GetPagedListEmployeeQueryHandlerTests
{


    [Fact]
    public async Task Handle_Should_Return_List()
    {

        var repository =
            new Mock<IEmployeeRepository>();

        var mapper =
            new Mock<MapsterMapper.IMapper>();



        repository
            .Setup(x=>x.GetPagedListAsync(
                null,
                null,
                null,
                1,
                10,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (new List<CollectManagement.Domain.Employess.Employee>(),0));



        mapper
            .Setup(x=>x.Map<List<GetPagedListEmployeeDto>>(
                It.IsAny<object>()))
            .Returns(new List<GetPagedListEmployeeDto>());



        var handler =
            new GetPagedListEmployeeQueryHandler(
                repository.Object,
                mapper.Object);



        var result =
            await handler.Handle(
                new GetPagedListEmployeeQuery(
                    null,
                    null,
                    null,
                    1,
                    10),
                CancellationToken.None);



        result.Should().NotBeNull();

    }

}