using CollectManagement.Domain.JoursFeries;
using CollectManagement.Domain.JoursFeries.ValueObjects;
using FluentAssertions;

namespace CollectManagement.Tests.Features.Domain.JoursFeries;

public class JourFerieTests
{

    [Fact]
    public void Create_Should_Create_JourFerie()
    {

        var id   = new JourFerieId(Ulid.NewUlid());
        var date = new DateTime(2026, 1, 14);


        var jourFerie = JourFerie.Create(
            id,
            date,
            "Fête de la Révolution"
        );


        jourFerie.Should().NotBeNull();

        jourFerie.JourFerieId.Should().Be(id);

        jourFerie.Label.Should().Be("Fête de la Révolution");

        jourFerie.Date.Should().Be(date.Date);
    }


    [Fact]
    public void Create_Should_Store_Only_Date_Part()
    {

        var dateWithTime = new DateTime(2026, 3, 20, 14, 30, 0);


        var jourFerie = JourFerie.Create(
            new JourFerieId(Ulid.NewUlid()),
            dateWithTime,
            "Equinoxe"
        );


        jourFerie.Date.TimeOfDay.Should().Be(TimeSpan.Zero);
    }


    [Fact]
    public void Update_Should_Modify_JourFerie()
    {

        var jourFerie = JourFerie.Create(
            new JourFerieId(Ulid.NewUlid()),
            new DateTime(2026, 1, 14),
            "Old Label"
        );


        jourFerie.Update(
            new DateTime(2026, 8, 13),
            "New Label"
        );


        jourFerie.Label.Should().Be("New Label");

        jourFerie.Date.Should().Be(new DateTime(2026, 8, 13).Date);
    }
}
