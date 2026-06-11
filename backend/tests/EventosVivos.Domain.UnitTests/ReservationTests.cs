using EventosVivos.Domain.Common;
using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Enums;
using EventosVivos.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace EventosVivos.Domain.UnitTests;

public class ReservationTests
{
    private static Reservation NewReservation(int quantity = 2)
        => Reservation.Create(Guid.NewGuid(), quantity, "Ana Pérez", Email.Create("ana@example.com"), TestData.Now);

    [Fact]
    public void Create_QuantityZero_Throws()
    {
        var act = () => NewReservation(quantity: 0);
        act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be("RESERVATION_QUANTITY_MIN");
    }

    [Fact]
    public void Create_StartsPendingPayment()
        => NewReservation().Status.Should().Be(ReservationStatus.PendientePago);

    [Fact]
    public void ConfirmPayment_FromPending_Confirms()
    {
        var r = NewReservation();
        r.ConfirmPayment("EV-123456", TestData.Now);
        r.Status.Should().Be(ReservationStatus.Confirmada);
        r.ReservationCode.Should().Be("EV-123456");
        r.ConfirmedAtUtc.Should().Be(TestData.Now);
    }

    [Fact]
    public void ConfirmPayment_AlreadyConfirmed_Throws()
    {
        var r = NewReservation();
        r.ConfirmPayment("EV-123456", TestData.Now);
        var act = () => r.ConfirmPayment("EV-999999", TestData.Now);
        act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be("RESERVATION_ALREADY_CONFIRMED");
    }

    [Fact]
    public void ConfirmPayment_Cancelled_Throws()
    {
        var r = NewReservation();
        r.Cancel(eventStartUtc: TestData.Now.AddDays(10), nowUtc: TestData.Now); // becomes Cancelada
        var act = () => r.ConfirmPayment("EV-123456", TestData.Now);
        act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be("RESERVATION_NOT_PAYABLE");
    }

    [Fact]
    public void Cancel_PendingReservation_ReleasesTickets()
    {
        var r = NewReservation();
        r.Cancel(eventStartUtc: TestData.Now.AddDays(10), nowUtc: TestData.Now);
        r.Status.Should().Be(ReservationStatus.Cancelada);
        r.CancelledAtUtc.Should().Be(TestData.Now);
    }

    [Fact]
    public void Cancel_ConfirmedMoreThan48hBefore_BecomesCancelada()
    {
        var r = NewReservation();
        r.ConfirmPayment("EV-123456", TestData.Now);
        r.Cancel(eventStartUtc: TestData.Now.AddHours(72), nowUtc: TestData.Now);
        r.Status.Should().Be(ReservationStatus.Cancelada);
    }

    [Fact]
    public void Cancel_ConfirmedWithin48h_BecomesPerdida_RN07()
    {
        var r = NewReservation();
        r.ConfirmPayment("EV-123456", TestData.Now);
        r.Cancel(eventStartUtc: TestData.Now.AddHours(24), nowUtc: TestData.Now);
        r.Status.Should().Be(ReservationStatus.Perdida);
    }

    [Fact]
    public void Cancel_AlreadyCancelled_Throws()
    {
        var r = NewReservation();
        r.Cancel(eventStartUtc: TestData.Now.AddDays(10), nowUtc: TestData.Now);
        var act = () => r.Cancel(eventStartUtc: TestData.Now.AddDays(10), nowUtc: TestData.Now);
        act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be("RESERVATION_ALREADY_CLOSED");
    }
}
