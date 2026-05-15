namespace SolucionPruebaTecnica.Domain.Interfaces;

public interface IOAuthTokenService
{
    public Task<string> GetAuthTokenAsync(CancellationToken cancellationToken = default);
}
