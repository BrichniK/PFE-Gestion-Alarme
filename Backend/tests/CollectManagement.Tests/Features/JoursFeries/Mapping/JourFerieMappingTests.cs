using CollectManagement.Application.Features.JoursFeries.Commands.CreateJourFerie;
using CollectManagement.Application.Features.JoursFeries.Mapping;
using CollectManagement.Application.Features.JoursFeries.Queries.GetOneJourFerie;
using CollectManagement.Domain.JoursFeries;
using CollectManagement.Domain.JoursFeries.ValueObjects;
using FluentAssertions;
using Mapster;

namespace CollectManagement.Tests.Features.JoursFeries.Mapping;

public class JourFerieMappingTests
{
    private readonly TypeAdapterConfig _config;

    public JourFerieMappingTests()
    {
        _config = new TypeAdapterConfig();
        _config.Scan(typeof(JourFerieMapping).Assembly);
    }

    [Fact]
    public void ShouldMapJourFerieToCreateJourFerieResponse()
    {
        // Arrange
        var jourFerieId = new JourFerieId(Ulid.NewUlid());
        var jourFerie = JourFerie.Create(jourFerieId, new DateTime(2026, 1, 1), "Jour de l'an");

        // Act
        var result = jourFerie.Adapt<CreateJourFerieResponse>(_config);

        // Assert
        result.Should().NotBeNull();
        result.JourFerieId.Should().Be(jourFerieId.Value);
    }

    [Fact]
    public void ShouldMapJourFerieToGetOneJourFerieResponse()
    {
        // Arrange
        var jourFerieId = new JourFerieId(Ulid.NewUlid());
        var jourFerie = JourFerie.Create(jourFerieId, new DateTime(2026, 1, 1), "Jour de l'an");

        // Act
        var result = jourFerie.Adapt<GetOneJourFerieResponse>(_config);

        // Assert
        result.Should().NotBeNull();
        result.JourFerieId.Should().Be(jourFerieId.Value);
        result.Label.Should().Be("Jour de l'an");
    }
}
