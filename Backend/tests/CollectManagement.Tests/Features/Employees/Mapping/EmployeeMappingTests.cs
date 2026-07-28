using CollectManagement.Application.Features.Employees.Commands.CreateEmployee;
using CollectManagement.Application.Features.Employees.Mapping;
using CollectManagement.Application.Features.Employees.Queries.GetOneEmployee;
using CollectManagement.Application.Features.Employees.Queries.GetPagedListEmployee;
using CollectManagement.Domain.Employess;
using CollectManagement.Domain.Employess.ObjectValues;
using FluentAssertions;
using Mapster;

namespace CollectManagement.Tests.Features.Employees.Mapping;


public class EmployeeMappingTests
{

    private readonly TypeAdapterConfig _config;


    public EmployeeMappingTests()
    {
        _config = new TypeAdapterConfig();

        new EmployeeMapping()
            .Register(_config);
    }



    private Employee CreateEmployee()
    {

        return Employee.Create(
            new EmployeeId(Ulid.NewUlid()),
            "Khalil",
            "Brichni",
            71111111,
            "RFID001",
            "test@test.com",
            "logo.png"
        );

    }



    [Fact]
    public void Should_Map_To_CreateResponse()
    {

        var employee = CreateEmployee();


        var result =
            employee.Adapt<CreateEmployeeResponse>(_config);


        result.EmployeeId
            .Should()
            .Be(employee.EmployeeId.Value);

    }



    [Fact]
    public void Should_Map_To_GetOneResponse()
    {

        var employee = CreateEmployee();


        var result =
            employee.Adapt<GetOneEmployeeResponse>(_config);


        result.EmployeeId
            .Should()
            .Be(employee.EmployeeId.Value);

    }



    [Fact]
    public void Should_Map_To_ListDto()
    {

        var employee = CreateEmployee();


        var result =
            employee.Adapt<GetPagedListEmployeeDto>(_config);


        result.EmployeeId
            .Should()
            .Be(employee.EmployeeId.Value);

    }

}