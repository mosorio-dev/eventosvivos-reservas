using EventosVivos.Application.IntegrationTests.Infrastructure;
using EventosVivos.Application.Reports.Queries.GetOccupancyReport;
using EventosVivos.Application.Reservations.Commands.ConfirmReservationPayment;
using EventosVivos.Application.Reservations.Commands.CreateReservation;
using FluentAssertions;
using Xunit;

namespace EventosVivos.Application.IntegrationTests;

public class OccupancyReportTests : HandlerTestBase
{
    [Fact]
    public async Task Report_ComputesSoldAvailableOccupancyAndRevenue_RF06()
    {
        var ev = await GivenEventAsync(capacity: 10, price: 30m);

        var reserve = new CreateReservationCommandHandler(Db, Clock);
        var confirm = new ConfirmReservationPaymentCommandHandler(Db, Clock, CodeGenerator);

        // 4 confirmed, 2 pending.
        var r1 = await reserve.Handle(new CreateReservationCommand(ev.Id, 4, "Ana", "ana@example.com"), CancellationToken.None);
        await confirm.Handle(new ConfirmReservationPaymentCommand(r1.Id), CancellationToken.None);
        await reserve.Handle(new CreateReservationCommand(ev.Id, 2, "Beto", "beto@example.com"), CancellationToken.None);

        var report = await new GetOccupancyReportQueryHandler(Db, Clock)
            .Handle(new GetOccupancyReportQuery(ev.Id), CancellationToken.None);

        report.TicketsSold.Should().Be(4);
        report.TicketsPending.Should().Be(2);
        report.TicketsAvailable.Should().Be(4);   // 10 - (4 + 2)
        report.OccupancyPercentage.Should().Be(60m);
        report.TotalRevenue.Should().Be(120m);     // 30 * 4 confirmed
    }
}
