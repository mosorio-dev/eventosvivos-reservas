using EventosVivos.Application.Events.Queries.ListEvents;
using EventosVivos.Application.IntegrationTests.Infrastructure;
using EventosVivos.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace EventosVivos.Application.IntegrationTests;

public class ListEventsTests : HandlerTestBase
{
    private ListEventsQueryHandler Handler() => new(Db, Clock);

    [Fact]
    public async Task List_FilterByType_ReturnsOnlyMatching()
    {
        await GivenEventAsync(venueId: 1, type: EventType.Conferencia, start: Clock.UtcNow.AddDays(3));
        await GivenEventAsync(venueId: 3, type: EventType.Concierto, start: Clock.UtcNow.AddDays(4));

        var result = await Handler().Handle(new ListEventsQuery(Type: EventType.Concierto), CancellationToken.None);

        result.Should().ContainSingle().Which.Type.Should().Be(EventType.Concierto);
    }

    [Fact]
    public async Task List_FilterByTitle_IsCaseInsensitive()
    {
        await GivenEventAsync(venueId: 1, start: Clock.UtcNow.AddDays(3));
        var result = await Handler().Handle(new ListEventsQuery(Title: "PRUEBA"), CancellationToken.None);
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task List_FilterByCompletedStatus_UsesDerivedState_RN06()
    {
        var ev = await GivenEventAsync(venueId: 1, start: Clock.UtcNow.AddDays(1), end: Clock.UtcNow.AddDays(1).AddHours(2));

        // Advance the clock beyond the event end.
        Clock.UtcNow = ev.EndUtc.AddHours(1);

        var completed = await Handler().Handle(new ListEventsQuery(Status: EventStatus.Completado), CancellationToken.None);
        completed.Should().ContainSingle(e => e.Id == ev.Id);

        var active = await Handler().Handle(new ListEventsQuery(Status: EventStatus.Activo), CancellationToken.None);
        active.Should().BeEmpty();
    }
}
