namespace EventosVivos.Api.Contracts;

/// <summary>HTTP payload for RF-03.</summary>
public sealed record CreateReservationRequest(
    Guid EventId,
    int Quantity,
    string BuyerName,
    string BuyerEmail);
