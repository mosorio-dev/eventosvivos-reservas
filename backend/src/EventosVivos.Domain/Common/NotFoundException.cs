namespace EventosVivos.Domain.Common;

/// <summary>Thrown when an aggregate referenced by id does not exist.</summary>
public sealed class NotFoundException : DomainException
{
    public NotFoundException(string entity, object key)
        : base($"{entity} con identificador '{key}' no fue encontrado.")
    {
    }
}
