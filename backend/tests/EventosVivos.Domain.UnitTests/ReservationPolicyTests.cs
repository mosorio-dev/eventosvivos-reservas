using EventosVivos.Domain.Common;
using EventosVivos.Domain.Services;
using FluentAssertions;
using Xunit;

namespace EventosVivos.Domain.UnitTests;

public class ReservationPolicyTests
{
    private static readonly DateTime Now = TestData.Now;

    [Fact]
    public void EnsureReservationWindowOpen_LessThanOneHour_Throws_RN04()
    {
        var act = () => ReservationPolicy.EnsureReservationWindowOpen(Now.AddMinutes(30), Now);
        act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be("RESERVATION_WINDOW_CLOSED");
    }

    [Fact]
    public void EnsureReservationWindowOpen_MoreThanOneHour_Ok()
    {
        var act = () => ReservationPolicy.EnsureReservationWindowOpen(Now.AddHours(2), Now);
        act.Should().NotThrow();
    }

    [Fact]
    public void MaxTickets_ExpensiveEventFarAway_Is10_RN05()
    {
        var max = ReservationPolicy.MaxTicketsPerTransaction(price: 150m, eventStartUtc: Now.AddDays(10), nowUtc: Now);
        max.Should().Be(10);
    }

    [Fact]
    public void MaxTickets_CheapImminentEvent_Is5_RF03()
    {
        var max = ReservationPolicy.MaxTicketsPerTransaction(price: 20m, eventStartUtc: Now.AddHours(10), nowUtc: Now);
        max.Should().Be(5);
    }

    [Fact]
    public void MaxTickets_ExpensiveAndImminent_StrictestWins_Is5()
    {
        var max = ReservationPolicy.MaxTicketsPerTransaction(price: 150m, eventStartUtc: Now.AddHours(10), nowUtc: Now);
        max.Should().Be(5);
    }

    [Fact]
    public void EnsureQuantityWithinTransactionLimit_Exceeds_Throws()
    {
        var act = () => ReservationPolicy.EnsureQuantityWithinTransactionLimit(11, price: 150m, eventStartUtc: Now.AddDays(10), nowUtc: Now);
        act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be("RESERVATION_QUANTITY_LIMIT");
    }
}
