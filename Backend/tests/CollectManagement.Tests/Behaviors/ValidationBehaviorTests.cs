using CollectManagement.Application.Behaviors;
using CollectManagement.Application.Exceptions;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace CollectManagement.Tests.Behaviors;

public class ValidationBehaviorTests
{
    private readonly Mock<ILogger<ValidationBehavior<ValidatableRequest, TestValidationResponse>>> _logger;

    public ValidationBehaviorTests()
    {
        _logger = new Mock<ILogger<ValidationBehavior<ValidatableRequest, TestValidationResponse>>>();
    }

    [Fact]
    public async Task Handle_Should_Call_Next_When_No_Validator()
    {
        var behavior = new ValidationBehavior<ValidatableRequest, TestValidationResponse>(
            _logger.Object, validator: null);

        var expected = new TestValidationResponse();
        RequestHandlerDelegate<TestValidationResponse> next = () => Task.FromResult(expected);

        var result = await behavior.Handle(new ValidatableRequest(), next, CancellationToken.None);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task Handle_Should_Call_Next_When_Validation_Passes()
    {
        var validator = new Mock<IValidator<ValidatableRequest>>();
        validator
            .Setup(v => v.ValidateAsync(It.IsAny<ValidatableRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var behavior = new ValidationBehavior<ValidatableRequest, TestValidationResponse>(
            _logger.Object, validator.Object);

        var expected = new TestValidationResponse();
        RequestHandlerDelegate<TestValidationResponse> next = () => Task.FromResult(expected);

        var result = await behavior.Handle(new ValidatableRequest(), next, CancellationToken.None);

        result.Should().Be(expected);
    }

    [Fact]
    public async Task Handle_Should_Throw_CustomValidationException_When_Validation_Fails()
    {
        var failures = new List<ValidationFailure>
        {
            new("Name", "Name is required"),
            new("Code", "Code is required")
        };

        var validator = new Mock<IValidator<ValidatableRequest>>();
        validator
            .Setup(v => v.ValidateAsync(It.IsAny<ValidatableRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));

        var behavior = new ValidationBehavior<ValidatableRequest, TestValidationResponse>(
            _logger.Object, validator.Object);

        RequestHandlerDelegate<TestValidationResponse> next = () => Task.FromResult(new TestValidationResponse());

        var act = async () => await behavior.Handle(new ValidatableRequest(), next, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<CustomValidationException>();
        ex.Which.ValdationErrors.Should().HaveCount(2);
        ex.Which.ValdationErrors.Should().Contain("Name is required");
    }
}

public record ValidatableRequest : IRequest<TestValidationResponse>;
public class TestValidationResponse { }
