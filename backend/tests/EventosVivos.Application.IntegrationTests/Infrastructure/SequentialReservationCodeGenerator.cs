using EventosVivos.Application.Common.Interfaces;

namespace EventosVivos.Application.IntegrationTests.Infrastructure;

public sealed class SequentialReservationCodeGenerator : IReservationCodeGenerator
{
    private int _counter;

    public string Next() => $"EV-{Interlocked.Increment(ref _counter):D6}";
}
