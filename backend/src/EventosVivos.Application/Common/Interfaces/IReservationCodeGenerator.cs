namespace EventosVivos.Application.Common.Interfaces;

/// <summary>Generates candidate reservation codes in the format EV-{6 digits}.</summary>
public interface IReservationCodeGenerator
{
    string Next();
}
