using EventosVivos.Domain.Common;
using EventosVivos.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace EventosVivos.Domain.UnitTests;

public class EmailTests
{
    [Fact]
    public void Create_ValidEmail_NormalizesToLowercase()
        => Email.Create("Ana.Perez@Example.COM").Value.Should().Be("ana.perez@example.com");

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missing@domain")]
    [InlineData("@nouser.com")]
    [InlineData("")]
    public void Create_InvalidEmail_Throws(string raw)
    {
        var act = () => Email.Create(raw);
        act.Should().Throw<BusinessRuleViolationException>().Which.Code.Should().Be("RESERVATION_INVALID_EMAIL");
    }
}
