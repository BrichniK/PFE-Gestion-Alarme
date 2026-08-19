using CollectManagement.Application.Exceptions;
using FluentAssertions;
using FluentValidation.Results;

namespace CollectManagement.Tests.Common;

public class CustomValidationExceptionTests
{
    [Fact]
    public void Constructor_With_Null_Result_Should_Have_Empty_Errors()
    {
        var ex = new CustomValidationException(null);

        ex.ValdationErrors.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_With_Failures_Should_Populate_Errors()
    {
        var failures = new List<ValidationFailure>
        {
            new("Name", "Name is required"),
            new("Code", "Code must be unique")
        };

        var result = new ValidationResult(failures);
        var ex     = new CustomValidationException(result);

        ex.ValdationErrors.Should().HaveCount(2);
        ex.ValdationErrors.Should().Contain("Name is required");
        ex.ValdationErrors.Should().Contain("Code must be unique");
    }

    [Fact]
    public void Constructor_With_Empty_Failures_Should_Have_Empty_Errors()
    {
        var result = new ValidationResult();
        var ex     = new CustomValidationException(result);

        ex.ValdationErrors.Should().BeEmpty();
    }
}
