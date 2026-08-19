using CollectManagement.Application.Exceptions;
using FluentAssertions;

namespace CollectManagement.Tests.Common;

public class ExceptionTests
{
    [Fact]
    public void NotFoundException_Should_Have_Correct_Message()
    {
        var ex = new NotFoundException("Entity not found");

        ex.Message.Should().Be("Entity not found");
    }

    [Fact]
    public void NotFoundException_With_Name_And_Key_Should_Format_Message()
    {
        var ex = new NotFoundException("Device", "123");

        ex.Message.Should().Contain("Device");
        ex.Message.Should().Contain("123");
    }

    [Fact]
    public void BadRequestException_Should_Have_Correct_Message()
    {
        var ex = new BadRequestException("Bad request data");

        ex.Message.Should().Be("Bad request data");
    }

    [Fact]
    public void ForbiddenException_Should_Have_Correct_Message()
    {
        var ex = new ForbiddenException("Access denied");

        ex.Message.Should().Be("Access denied");
    }

    [Fact]
    public void UnAuthorizedException_Should_Have_Correct_Message()
    {
        var ex = new UnAuthorizedException("Unauthorized");

        ex.Message.Should().Be("Unauthorized");
    }

    [Fact]
    public void BadCredentialException_Should_Have_Correct_Message()
    {
        var ex = new BadCredentialException("Invalid credentials");

        ex.Message.Should().Be("Invalid credentials");
    }
}
