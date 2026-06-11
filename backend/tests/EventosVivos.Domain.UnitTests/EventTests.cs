using EventosVivos.Domain.Common;
using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace EventosVivos.Domain.UnitTests;

public class EventTests
{
    private static Event CreateValid(
        DateTime? start = null,
        DateTime? end = null,
        int capacity = 100,
        Venue? venue = null,
        decimal price = 50m)
    {
        venue ??= TestData.AuditorioCentral();
        var s = start ?? TestData.Now.AddDays(10);
        var e = end ?? s.AddHours(2);
        return Event.Create("Conferencia .NET", "Una charla técnica sobre arquitectura.",
            venue, capacity, s, e, price, EventType.Conferencia, TestData.Now);
    }

    [Fact]
    public void Create_WithValidData_Succeeds()
    {
        var ev = CreateValid();
        ev.Status.Should().Be(EventStatus.Activo);
        ev.Capacity.Should().Be(100);
    }

    [Theory]
    [InlineData("abc")]                 // too short
    [InlineData("")]                    // empty
    public void Create_TitleTooShort_Throws(string title)
    {
        var act = () => Event.Create(title, "Descripción válida y suficiente.",
            TestData.AuditorioCentral(), 100, TestData.Now.AddDays(1), TestData.Now.AddDays(1).AddHours(2),
            50m, EventType.Taller, TestData.Now);

        act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be("EVENT_TITLE_LENGTH");
    }

    [Fact]
    public void Create_CapacityExceedsVenue_Throws_RN01()
    {
        // Sala Norte capacity = 50.
        var act = () => CreateValid(capacity: 51, venue: TestData.SalaNorte());
        act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be("EVENT_CAPACITY_EXCEEDS_VENUE");
    }

    [Fact]
    public void Create_CapacityEqualToVenue_Succeeds_RN01_Boundary()
    {
        var ev = CreateValid(capacity: 50, venue: TestData.SalaNorte());
        ev.Capacity.Should().Be(50);
    }

    [Fact]
    public void Create_StartInPast_Throws()
    {
        var act = () => CreateValid(start: TestData.Now.AddHours(-1), end: TestData.Now.AddHours(1));
        act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be("EVENT_START_IN_PAST");
    }

    [Fact]
    public void Create_EndBeforeStart_Throws()
    {
        var start = TestData.Now.AddDays(5);
        var act = () => CreateValid(start: start, end: start.AddHours(-1));
        act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be("EVENT_END_BEFORE_START");
    }

    [Fact]
    public void Create_NonPositivePrice_Throws()
    {
        var act = () => CreateValid(price: 0m);
        act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be("EVENT_PRICE_POSITIVE");
    }

    [Fact]
    public void Create_WeekendAfter10pm_Throws_RN03()
    {
        // 2026-01-03 is a Saturday, 23:00.
        var start = new DateTime(2026, 1, 3, 23, 0, 0, DateTimeKind.Utc);
        var act = () => CreateValid(start: start, end: start.AddHours(1));
        act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be("EVENT_WEEKEND_NIGHT");
    }

    [Fact]
    public void Create_WeekendBefore10pm_Succeeds_RN03()
    {
        var start = new DateTime(2026, 1, 3, 20, 0, 0, DateTimeKind.Utc);
        var ev = CreateValid(start: start, end: start.AddHours(1));
        ev.StartUtc.Should().Be(start);
    }

    [Fact]
    public void GetEffectiveStatus_PastEnd_ReturnsCompletado_RN06()
    {
        var start = TestData.Now.AddDays(1);
        var ev = CreateValid(start: start, end: start.AddHours(2));
        ev.GetEffectiveStatus(start.AddHours(3)).Should().Be(EventStatus.Completado);
    }

    [Fact]
    public void GetEffectiveStatus_Cancelled_StaysCancelled()
    {
        var ev = CreateValid();
        ev.Cancel();
        ev.GetEffectiveStatus(TestData.Now.AddYears(1)).Should().Be(EventStatus.Cancelado);
    }
}
