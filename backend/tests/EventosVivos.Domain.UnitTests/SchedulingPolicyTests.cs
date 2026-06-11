using EventosVivos.Domain.Services;
using FluentAssertions;
using Xunit;

namespace EventosVivos.Domain.UnitTests;

public class SchedulingPolicyTests
{
    private static readonly DateTime Base = new(2026, 5, 1, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Overlaps_WhenIntervalsIntersect_ReturnsTrue()
    {
        SchedulingPolicy.Overlaps(Base, Base.AddHours(2), Base.AddHours(1), Base.AddHours(3))
            .Should().BeTrue();
    }

    [Fact]
    public void Overlaps_WhenDisjoint_ReturnsFalse()
    {
        SchedulingPolicy.Overlaps(Base, Base.AddHours(1), Base.AddHours(2), Base.AddHours(3))
            .Should().BeFalse();
    }

    [Fact]
    public void Overlaps_BackToBack_ReturnsFalse()
    {
        SchedulingPolicy.Overlaps(Base, Base.AddHours(2), Base.AddHours(2), Base.AddHours(4))
            .Should().BeFalse();
    }
}
