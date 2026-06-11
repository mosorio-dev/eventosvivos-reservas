using EventosVivos.Application.Common.Interfaces;

namespace EventosVivos.Api.Auth;

/// <summary>
/// Validates admin credentials against configuration. For this scope a single admin
/// account is enough; a production system would use a user store with hashed passwords.
/// </summary>
public sealed class AdminAuthenticator : IAdminAuthenticator
{
    private readonly string _username;
    private readonly string _password;

    public AdminAuthenticator(IConfiguration configuration)
    {
        _username = configuration["Auth:AdminUsername"] ?? "admin";
        _password = configuration["Auth:AdminPassword"] ?? "Admin123!";
    }

    public bool ValidateCredentials(string username, string password)
        => string.Equals(username, _username, StringComparison.Ordinal)
           && string.Equals(password, _password, StringComparison.Ordinal);
}
