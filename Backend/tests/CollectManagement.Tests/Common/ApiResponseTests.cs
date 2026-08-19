using CollectManagement.Application.Common;
using FluentAssertions;
using System.Net;

namespace CollectManagement.Tests.Common;

public class ApiResponseTests
{
    [Fact]
    public void Default_Constructor_Should_Set_Success_And_200()
    {
        var response = new ApiResponse<string>();

        response.Success.Should().BeTrue();
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response.Data.Should().BeNull();
        response.Message.Should().BeNull();
    }

    [Fact]
    public void Constructor_With_Message_Should_Set_Message()
    {
        var response = new ApiResponse<string>("Operation réussie");

        response.Success.Should().BeTrue();
        response.Message.Should().Be("Operation réussie");
        response.StatusCode.Should().Be(200);
    }

    [Fact]
    public void Constructor_With_Message_Success_StatusCode_Should_Set_All()
    {
        var response = new ApiResponse<string>("Erreur", false, 400);

        response.Success.Should().BeFalse();
        response.Message.Should().Be("Erreur");
        response.StatusCode.Should().Be(400);
    }

    [Fact]
    public void Constructor_With_Data_Should_Set_Data()
    {
        var response = new ApiResponse<int>(42, "ok");

        response.Data.Should().Be(42);
        response.Message.Should().Be("ok");
        response.Success.Should().BeTrue();
    }

    [Fact]
    public void Constructor_With_Data_No_Message_Should_Work()
    {
        var response = new ApiResponse<string>("hello");

        response.Success.Should().BeTrue();
    }
}
