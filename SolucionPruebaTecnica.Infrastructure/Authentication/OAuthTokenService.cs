using Microsoft.Extensions.Options;
using SolucionPruebaTecnica.Domain.Interfaces;
using SolucionPruebaTecnica.Infrastructure.Common.DTO;
using SolucionPruebaTecnica.Infrastructure.Configuration;
using System.Net.Http.Json;

namespace SolucionPruebaTecnica.Infrastructure.Authentication;

public class OAuthTokenService : IOAuthTokenService
{
    private readonly PowerAutomateOptions _powerAutomateOptions;
    private readonly HttpClient _httpClient;
    private string? _cachedToken;
    private DateTime _tokenExpiration;
    public OAuthTokenService(IOptions<PowerAutomateOptions> powerAutomateOptions, HttpClient httpClient)
    {
        _powerAutomateOptions = powerAutomateOptions.Value;
        _httpClient = httpClient;
    }
    public async Task<string> GetAuthTokenAsync(CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(_cachedToken) && DateTime.UtcNow < _tokenExpiration)
        {
            return _cachedToken;
        }

        var tokenRequest = new Dictionary<string, string>
        {
            ["client_id"] = _powerAutomateOptions.ClientId,
            ["client_secret"] = _powerAutomateOptions.ClientSecret,
            ["scope"] = _powerAutomateOptions.Scope,
            ["tenant"] = _powerAutomateOptions.TenantId,
            ["grant_type"] = "client_credentials"
        };

        var url = $"{_powerAutomateOptions.AuthUrl}/oauth2/v2.0/token";

        var content = new FormUrlEncodedContent(tokenRequest);

        var response = await _httpClient.PostAsync(url, content, cancellationToken);

        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken);

        if (tokenResponse is null || string.IsNullOrWhiteSpace(tokenResponse.access_token))
        {
            throw new Exception("Failed to retrieve access token.");
        }

        _cachedToken = tokenResponse.access_token;
        _tokenExpiration = DateTime.UtcNow.AddSeconds(tokenResponse.expires_in);

        return _cachedToken;
    }
}
