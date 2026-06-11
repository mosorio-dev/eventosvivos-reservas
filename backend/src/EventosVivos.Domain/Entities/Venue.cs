namespace EventosVivos.Domain.Entities;

/// <summary>
/// Pre-existing physical location. Seeded reference data (RN01 capacity constraint source).
/// Uses an int identity to match the provided reference catalog (1, 2, 3).
/// </summary>
public class Venue
{
    public int Id { get; private set; }
    public string Name { get; private set; } = null!;
    public int Capacity { get; private set; }
    public string City { get; private set; } = null!;

    private Venue() { } // EF Core

    public Venue(int id, string name, int capacity, string city)
    {
        Id = id;
        Name = name;
        Capacity = capacity;
        City = city;
    }
}
