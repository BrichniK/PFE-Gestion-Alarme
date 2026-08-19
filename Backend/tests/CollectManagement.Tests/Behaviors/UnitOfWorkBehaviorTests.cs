using CollectManagement.Application.Behaviors;
using CollectManagement.Application.Shared;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace CollectManagement.Tests.Behaviors;

public class UnitOfWorkBehaviorTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<UnitOfWorkBehavior<UoWCommand, UoWResponse>>> _commandLogger;
    private readonly Mock<ILogger<UnitOfWorkBehavior<UoWQuery, UoWResponse>>> _queryLogger;

    public UnitOfWorkBehaviorTests()
    {
        _unitOfWork    = new Mock<IUnitOfWork>();
        _commandLogger = new Mock<ILogger<UnitOfWorkBehavior<UoWCommand, UoWResponse>>>();
        _queryLogger   = new Mock<ILogger<UnitOfWorkBehavior<UoWQuery, UoWResponse>>>();
    }

    [Fact]
    public async Task Handle_Should_SaveChanges_And_Commit_For_Command()
    {
        _unitOfWork
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWork
            .Setup(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var behavior = new UnitOfWorkBehavior<UoWCommand, UoWResponse>(
            _unitOfWork.Object, _commandLogger.Object);

        var expected = new UoWResponse();
        RequestHandlerDelegate<UoWResponse> next = () => Task.FromResult(expected);

        var result = await behavior.Handle(new UoWCommand(), next, CancellationToken.None);

        result.Should().Be(expected);

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Skip_Transaction_For_Query()
    {
        var behavior = new UnitOfWorkBehavior<UoWQuery, UoWResponse>(
            _unitOfWork.Object, _queryLogger.Object);

        var expected = new UoWResponse();
        RequestHandlerDelegate<UoWResponse> next = () => Task.FromResult(expected);

        var result = await behavior.Handle(new UoWQuery(), next, CancellationToken.None);

        result.Should().Be(expected);

        _unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

// Command — name ends with "Command"
public record UoWCommand : IRequest<UoWResponse>;

// Query — name does NOT end with "Command"
public record UoWQuery : IRequest<UoWResponse>;

public class UoWResponse { }
