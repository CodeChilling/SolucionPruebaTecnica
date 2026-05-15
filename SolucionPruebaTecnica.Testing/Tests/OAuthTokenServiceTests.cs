using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using SolucionPruebaTecnica.Infrastructure.Authentication;
using SolucionPruebaTecnica.Infrastructure.Common.DTO;
using SolucionPruebaTecnica.Infrastructure.Configuration;
using System.Net;
using System.Net.Http.Json;

namespace SolucionPruebaTecnica.Testing.Tests;

public class MockHttpMessageHandlerForOAuth : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _sendAsync;

    public MockHttpMessageHandlerForOAuth(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
    {
        _sendAsync = sendAsync;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return _sendAsync(request, cancellationToken);
    }
}

public class OAuthTokenServiceTests
{
    private readonly Mock<IOptions<PowerAutomateOptions>> _optionsMock;
    private readonly PowerAutomateOptions _options;

    public OAuthTokenServiceTests()
    {
        _options = new PowerAutomateOptions
        {
            BaseUrl = "https://api.powerautomate.com/customers",
            ClientId = "test-client-id",
            ClientSecret = "test-client-secret",
            TenantId = "test-tenant-id",
            Scope = "https://api.powerautomate.com/.default",
            AuthUrl = "https://login.microsoftonline.com"
        };
        _optionsMock = new Mock<IOptions<PowerAutomateOptions>>();
        _optionsMock.Setup(x => x.Value).Returns(_options);
    }

    private OAuthTokenService CreateSut(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handlerFunc)
    {
        var handler = new MockHttpMessageHandlerForOAuth(handlerFunc);
        var httpClient = new HttpClient(handler);
        return new OAuthTokenService(_optionsMock.Object, httpClient);
    }

    [Fact]
    public async Task GetAuthTokenAsync_SinTokenCacheado_ObtieneNuevoToken()
    {
        var tokenResponse = new TokenResponse
        {
            access_token = "new-access-token",
            token_type = "Bearer",
            expires_in = 3600
        };

        var sut = CreateSut(async (request, cancellationToken) =>
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(tokenResponse)
            };
        });

        var result = await sut.GetAuthTokenAsync();

        result.Should().Be("new-access-token");
    }

    [Fact]
    public async Task GetAuthTokenAsync_ConTokenCacheadoValido_RetornaTokenCacheado()
    {
        var tokenResponse = new TokenResponse
        {
            access_token = "cached-token",
            token_type = "Bearer",
            expires_in = 3600
        };

        var callCount = 0;
        var sut = CreateSut(async (request, cancellationToken) =>
        {
            callCount++;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(tokenResponse)
            };
        });

        var firstToken = await sut.GetAuthTokenAsync();
        var secondToken = await sut.GetAuthTokenAsync();

        secondToken.Should().Be(firstToken);
        callCount.Should().Be(1);
    }

    [Fact]
    public async Task GetAuthTokenAsync_ConTokenExpirado_ObtieneNuevoToken()
    {
        var firstTokenResponse = new TokenResponse
        {
            access_token = "expired-token",
            token_type = "Bearer",
            expires_in = -10
        };

        var secondTokenResponse = new TokenResponse
        {
            access_token = "new-token",
            token_type = "Bearer",
            expires_in = 3600
        };

        var callCount = 0;
        var sut = CreateSut(async (request, cancellationToken) =>
        {
            callCount++;
            return callCount == 1
                ? new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(firstTokenResponse) }
                : new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(secondTokenResponse) };
        });

        var firstToken = await sut.GetAuthTokenAsync();

        await Task.Delay(50);

        var secondToken = await sut.GetAuthTokenAsync();

        secondToken.Should().Be("new-token");
        callCount.Should().Be(2);
    }

    [Fact]
    public async Task GetAuthTokenAsync_ConRespuestaError_ThrowsException()
    {
        var sut = CreateSut(async (request, cancellationToken) =>
        {
            return new HttpResponseMessage(HttpStatusCode.Unauthorized);
        });

        var act = () => sut.GetAuthTokenAsync();

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task GetAuthTokenAsync_ConRespuestaNull_ThrowsException()
    {
        var sut = CreateSut(async (request, cancellationToken) =>
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create<TokenResponse>(null!)
            };
        });

        var act = () => sut.GetAuthTokenAsync();

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Failed to retrieve access token.");
    }

    [Fact]
    public async Task GetAuthTokenAsync_ConTokenVacio_ThrowsException()
    {
        var tokenResponse = new TokenResponse
        {
            access_token = "",
            token_type = "Bearer",
            expires_in = 3600
        };

        var sut = CreateSut(async (request, cancellationToken) =>
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(tokenResponse)
            };
        });

        var act = () => sut.GetAuthTokenAsync();

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Failed to retrieve access token.");
    }

    [Fact]
    public async Task GetAuthTokenAsync_ConSolicitudCancelada_ThrowsOperationCanceledException()
    {
        var sut = CreateSut(async (request, cancellationToken) =>
        {
            throw new OperationCanceledException();
        });

        var act = () => sut.GetAuthTokenAsync();

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task GetAuthTokenAsync_EnviaSolicitudCorrecta_ConParametrosOAuth()
    {
        HttpRequestMessage? capturedRequest = null;

        var tokenResponse = new TokenResponse
        {
            access_token = "test-token",
            token_type = "Bearer",
            expires_in = 3600
        };

        var sut = CreateSut(async (request, cancellationToken) =>
        {
            capturedRequest = request;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(tokenResponse)
            };
        });

        await sut.GetAuthTokenAsync();

        capturedRequest.Should().NotBeNull();
        capturedRequest!.RequestUri!.ToString().Should().Contain("/oauth2/v2.0/token");
        capturedRequest.Method.Should().Be(HttpMethod.Post);
    }
}