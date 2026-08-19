using CollectManagement.Application.Behaviors;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace CollectManagement.Tests.Behaviors;

public class LoggingBehaviorTests
{
    private readonly Mock<ILogger<LoggingBehavior<TestRequest, TestResponse>>> _logger;
    private readonly LoggingBehavior<TestRequest, TestResponse> _behavior;

    public LoggingBehaviorTests()
    {
        _logger   = new Mock<ILogger<LoggingBehavior<TestRequest, TestResponse>>>();
        _behavior = new LoggingBehavior<TestRequest, TestResponse>(_logger.Object);
    }

    [Fact]
    public async Task Handle_Should_Call_Next_And_Return_Response()
    {
        var expected = new TestResponse { Value = "ok" };
        RequestHandlerDelegate<TestResponse> next = () => Task.FromResult(expected);

        var result = await _behavior.Handle(new TestRequest(), next, CancellationToken.None);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task Handle_Should_Execute_Without_Exception()
    {
        RequestHandlerDelegate<TestResponse> next = () => Task.FromResult(new TestResponse());

        var act = async () => await _behavior.Handle(new TestRequest(), next, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}

public record TestRequest : IRequest<TestResponse>;
public class TestResponse { public string? Value { get; set; } }
