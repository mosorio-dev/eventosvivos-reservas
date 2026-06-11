using EventosVivos.Application.Events.Commands.CreateEvent;
using Microsoft.EntityFrameworkCore;
using EventosVivos.Application.IntegrationTests.Infrastructure;
using EventosVivos.Domain.Common;
using EventosVivos.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace EventosVivos.Application.IntegrationTests;

public class CreateEventHandlerTests : HandlerTestBase
{
    private CreateEventCommandHandler Handler() => new(Db, Clock);

    private CreateEventCommand ValidCommand(int venueId = 1, int capacity = 100, DateTime? start = null, DateTime? end = null)
    {
        var s = start ?? Clock.UtcNow.AddDays(5);
        var e = end ?? s.AddHours(2);
        return new CreateEventCommand("Conferencia de Arquitectura", "Charla técnica sobre diseño de software.",
            venueId, capacity, s, e, 80m, EventType.Conferencia);
    }

    [Fact]
    public async Task Create_ValidEvent_Persists()
    {
        var dto = await Handler().Handle(ValidCommand(), CancellationToken.None);

        dto.Id.Should().NotBeEmpty();
        dto.VenueName.Should().Be("Auditorio Central");
        (await Db.Events.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Create_VenueNotFound_Throws()
    {
        var act = () => Handler().Handle(ValidCommand(venueId: 99), CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Create_CapacityExceedsVenue_Throws_RN01()
    {
        // Sala Norte (id 2) capacity = 50.
        var act = () => Handler().Handle(ValidCommand(venueId: 2, capacity: 51), CancellationToken.None);
        (await act.Should().ThrowAsync<BusinessRuleViolationException>())
            .Which.Code.Should().Be("EVENT_CAPACITY_EXCEEDS_VENUE");
    }

    [Fact]
    public async Task Create_OverlappingSameVenue_Throws_RN02()
    {
        var start = Clock.UtcNow.AddDays(5);
        await Handler().Handle(ValidCommand(start: start, end: start.AddHours(3)), CancellationToken.None);

        var overlapping = ValidCommand(start: start.AddHours(1), end: start.AddHours(4));
        var act = () => Handler().Handle(overlapping, CancellationToken.None);

        (await act.Should().ThrowAsync<BusinessRuleViolationException>())
            .Which.Code.Should().Be("EVENT_VENUE_OVERLAP");
    }

    [Fact]
    public async Task Create_BackToBackSameVenue_Succeeds()
    {
        var start = Clock.UtcNow.AddDays(5);
        await Handler().Handle(ValidCommand(start: start, end: start.AddHours(2)), CancellationToken.None);

        // Starts exactly when the previous ends -> no overlap.
        var next = ValidCommand(start: start.AddHours(2), end: start.AddHours(4));
        var act = () => Handler().Handle(next, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Create_OverlapDifferentVenue_Succeeds()
    {
        var start = Clock.UtcNow.AddDays(5);
        await Handler().Handle(ValidCommand(venueId: 1, start: start, end: start.AddHours(3)), CancellationToken.None);

        var act = () => Handler().Handle(ValidCommand(venueId: 3, start: start, end: start.AddHours(3)), CancellationToken.None);
        await act.Should().NotThrowAsync();
    }
}
