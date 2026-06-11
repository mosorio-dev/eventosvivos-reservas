using System.Text.RegularExpressions;
using EventosVivos.Application.IntegrationTests.Infrastructure;
using EventosVivos.Application.Reservations.Commands.CancelReservation;
using EventosVivos.Application.Reservations.Commands.ConfirmReservationPayment;
using EventosVivos.Application.Reservations.Commands.CreateReservation;
using EventosVivos.Domain.Common;
using EventosVivos.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace EventosVivos.Application.IntegrationTests;

public class ReservationFlowTests : HandlerTestBase
{
    private CreateReservationCommandHandler ReserveHandler() => new(Db, Clock);
    private ConfirmReservationPaymentCommandHandler ConfirmHandler() => new(Db, Clock, CodeGenerator);
    private CancelReservationCommandHandler CancelHandler() => new(Db, Clock);

    [Fact]
    public async Task Reserve_Valid_CreatesPendingReservation()
    {
        var ev = await GivenEventAsync(capacity: 100);
        var dto = await ReserveHandler().Handle(
            new CreateReservationCommand(ev.Id, 2, "Ana Pérez", "ana@example.com"), CancellationToken.None);

        dto.Status.Should().Be(ReservationStatus.PendientePago);
        dto.BuyerEmail.Should().Be("ana@example.com");
    }

    [Fact]
    public async Task Reserve_ExceedingCapacity_Throws()
    {
        var ev = await GivenEventAsync(capacity: 5);
        var act = () => ReserveHandler().Handle(
            new CreateReservationCommand(ev.Id, 6, "Ana", "ana@example.com"), CancellationToken.None);

        (await act.Should().ThrowAsync<BusinessRuleViolationException>())
            .Which.Code.Should().Be("EVENT_INSUFFICIENT_CAPACITY");
    }

    [Fact]
    public async Task Reserve_PendingReservationHoldsInventory()
    {
        var ev = await GivenEventAsync(capacity: 5);
        await ReserveHandler().Handle(new CreateReservationCommand(ev.Id, 5, "Ana", "ana@example.com"), CancellationToken.None);

        var act = () => ReserveHandler().Handle(new CreateReservationCommand(ev.Id, 1, "Beto", "beto@example.com"), CancellationToken.None);
        (await act.Should().ThrowAsync<BusinessRuleViolationException>())
            .Which.Code.Should().Be("EVENT_INSUFFICIENT_CAPACITY");
    }

    [Fact]
    public async Task Reserve_AfterCancellation_InventoryIsReleased()
    {
        var ev = await GivenEventAsync(capacity: 5);
        var first = await ReserveHandler().Handle(new CreateReservationCommand(ev.Id, 5, "Ana", "ana@example.com"), CancellationToken.None);

        await CancelHandler().Handle(new CancelReservationCommand(first.Id), CancellationToken.None);

        var act = () => ReserveHandler().Handle(new CreateReservationCommand(ev.Id, 5, "Beto", "beto@example.com"), CancellationToken.None);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Reserve_LessThan24h_MoreThan5_Throws_RF03()
    {
        var ev = await GivenEventAsync(capacity: 100, start: Clock.UtcNow.AddHours(10), end: Clock.UtcNow.AddHours(12), price: 20m);
        var act = () => ReserveHandler().Handle(new CreateReservationCommand(ev.Id, 6, "Ana", "ana@example.com"), CancellationToken.None);

        (await act.Should().ThrowAsync<BusinessRuleViolationException>())
            .Which.Code.Should().Be("RESERVATION_QUANTITY_LIMIT");
    }

    [Fact]
    public async Task Reserve_LessThanOneHourToStart_Throws_RN04()
    {
        var ev = await GivenEventAsync(capacity: 100, start: Clock.UtcNow.AddMinutes(30), end: Clock.UtcNow.AddHours(2), price: 20m);
        var act = () => ReserveHandler().Handle(new CreateReservationCommand(ev.Id, 1, "Ana", "ana@example.com"), CancellationToken.None);

        (await act.Should().ThrowAsync<BusinessRuleViolationException>())
            .Which.Code.Should().Be("RESERVATION_WINDOW_CLOSED");
    }

    [Fact]
    public async Task Confirm_GeneratesCodeInExpectedFormat_RF04()
    {
        var ev = await GivenEventAsync(capacity: 10);
        var reservation = await ReserveHandler().Handle(new CreateReservationCommand(ev.Id, 1, "Ana", "ana@example.com"), CancellationToken.None);

        var confirmed = await ConfirmHandler().Handle(new ConfirmReservationPaymentCommand(reservation.Id), CancellationToken.None);

        confirmed.Status.Should().Be(ReservationStatus.Confirmada);
        confirmed.ReservationCode.Should().NotBeNull();
        Regex.IsMatch(confirmed.ReservationCode!, "^EV-\\d{6}$").Should().BeTrue();
    }

    [Fact]
    public async Task Confirm_AlreadyConfirmed_Throws()
    {
        var ev = await GivenEventAsync(capacity: 10);
        var reservation = await ReserveHandler().Handle(new CreateReservationCommand(ev.Id, 1, "Ana", "ana@example.com"), CancellationToken.None);
        await ConfirmHandler().Handle(new ConfirmReservationPaymentCommand(reservation.Id), CancellationToken.None);

        var act = () => ConfirmHandler().Handle(new ConfirmReservationPaymentCommand(reservation.Id), CancellationToken.None);
        (await act.Should().ThrowAsync<BusinessRuleViolationException>())
            .Which.Code.Should().Be("RESERVATION_ALREADY_CONFIRMED");
    }

    [Fact]
    public async Task Cancel_ConfirmedWithin48h_BecomesPerdida_AndKeepsInventory_RN07()
    {
        var ev = await GivenEventAsync(capacity: 5, start: Clock.UtcNow.AddHours(24), end: Clock.UtcNow.AddHours(26), price: 20m);

        // Reserve all 5 and confirm.
        var reservation = await ReserveHandler().Handle(new CreateReservationCommand(ev.Id, 5, "Ana", "ana@example.com"), CancellationToken.None);
        await ConfirmHandler().Handle(new ConfirmReservationPaymentCommand(reservation.Id), CancellationToken.None);

        var cancelled = await CancelHandler().Handle(new CancelReservationCommand(reservation.Id), CancellationToken.None);
        cancelled.Status.Should().Be(ReservationStatus.Perdida);

        // Inventory is NOT released, so a new reservation should fail.
        var act = () => ReserveHandler().Handle(new CreateReservationCommand(ev.Id, 1, "Beto", "beto@example.com"), CancellationToken.None);
        (await act.Should().ThrowAsync<BusinessRuleViolationException>())
            .Which.Code.Should().Be("EVENT_INSUFFICIENT_CAPACITY");
    }
}
