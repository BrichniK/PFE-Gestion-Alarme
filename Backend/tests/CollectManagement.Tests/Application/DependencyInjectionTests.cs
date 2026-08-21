using CollectManagement.Application;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CollectManagement.Tests.Application;

public class DependencyInjectionTests
{
    [Fact]
    public void AddApplicationServices_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddApplicationServices();

        // Assert
        Assert.NotNull(result);
        Assert.Same(services, result);

        Assert.Contains(
            services,
            descriptor => descriptor.ServiceType == typeof(IMapper));

        Assert.Contains(
            services,
            descriptor => descriptor.ServiceType == typeof(IMediator));
    }
}