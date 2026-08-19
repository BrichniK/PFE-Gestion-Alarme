using CollectManagement.Domain.Employess;
using CollectManagement.Domain.Employess.ObjectValues;
using FluentAssertions;

namespace CollectManagement.Tests.Features.Domain.Employees;

public class EmployeeTests
{

    [Fact]
    public void Create_Should_Create_Employee()
    {

        var id = new EmployeeId(Ulid.NewUlid());


        var employee = Employee.Create(
            id,
            "Ben Ali",
            "Mohamed",
            12345678,
            "RFID-001",
            "mohamed@example.com",
            "/logos/emp.png"
        );


        employee.Should().NotBeNull();

        employee.EmployeeId.Should().Be(id);

        employee.Nom.Should().Be("Ben Ali");

        employee.Prenom.Should().Be("Mohamed");

        employee.Phone.Should().Be(12345678);

        employee.Rfid.Should().Be("RFID-001");

        employee.Email.Should().Be("mohamed@example.com");

        employee.LogoPath.Should().Be("/logos/emp.png");
    }


    [Fact]
    public void Create_With_Null_Optional_Fields_Should_Be_Allowed()
    {

        var employee = Employee.Create(
            new EmployeeId(Ulid.NewUlid()),
            "Trabelsi",
            "Ali",
            98765432,
            "RFID-002",
            null,
            null
        );


        employee.Email.Should().BeNull();

        employee.LogoPath.Should().BeNull();
    }
}
