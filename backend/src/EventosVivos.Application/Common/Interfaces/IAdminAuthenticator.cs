namespace EventosVivos.Application.Common.Interfaces;

/// <summary>Validates administrator credentials (kept behind an interface to stay infra-agnostic).</summary>
public interface IAdminAuthenticator
{
    bool ValidateCredentials(string username, string password);
}
