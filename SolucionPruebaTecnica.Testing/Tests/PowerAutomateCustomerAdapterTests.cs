using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SolucionPruebaTecnica.Application.Common.DTO;
using SolucionPruebaTecnica.Domain.Entities;
using SolucionPruebaTecnica.Domain.Interfaces;
using SolucionPruebaTecnica.Infrastructure.Adapter;
using SolucionPruebaTecnica.Infrastructure.Configuration;
using System.Net;
using System.Net.Http.Json;

namespace SolucionPruebaTecnica.Testing.Tests;

public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _sendAsync;

    public MockHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
    {
        _sendAsync = sendAsync;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return _sendAsync(request, cancellationToken);
    }
}

public class PowerAutomateCustomerAdapterTests
{
    private readonly Mock<IOAuthTokenService> _oAuthTokenServiceMock;
    private readonly Mock<IOptions<PowerAutomateOptions>> _optionsMock;
    private readonly HttpClient _httpClient;
    private readonly Mock<ILogger<PowerAutomateCustomerAdapter>> _loggerMock;
    private readonly PowerAutomateOptions _options;

    public PowerAutomateCustomerAdapterTests()
    {
        _oAuthTokenServiceMock = new Mock<IOAuthTokenService>();
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
        _loggerMock = new Mock<ILogger<PowerAutomateCustomerAdapter>>();
    }

    private PowerAutomateCustomerAdapter CreateSut(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handlerFunc)
    {
        var handler = new MockHttpMessageHandler(handlerFunc);
        var httpClient = new HttpClient(handler);
        return new PowerAutomateCustomerAdapter(
            _oAuthTokenServiceMock.Object,
            _optionsMock.Object,
            httpClient,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetCustomerByIdAsync_ConClienteExistente_RetornaCliente()
    {
        var customerId = Guid.NewGuid().ToString();
        var token = "test-token";
        var customerResponse = new CustomerResponse
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            Age = 28,
            City = "Los Angeles"
        };

        _oAuthTokenServiceMock
            .Setup(x => x.GetAuthTokenAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        var sut = CreateSut(async (request, cancellationToken) =>
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(customerResponse)
            };
        });

        var result = await sut.GetCustomerByIdAsync(customerId);

        result.Should().NotBeNull();
        result!.FirstName.Should().Be("Jane");
        result.LastName.Should().Be("Smith");
        result.Email.Should().Be("jane.smith@example.com");
        result.Age.Should().Be(28);
        result.City.Should().Be("Los Angeles");

        _oAuthTokenServiceMock.Verify(
            x => x.GetAuthTokenAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetCustomerByIdAsync_ConClienteNoExistente_RetornaNull()
    {
        var customerId = Guid.NewGuid().ToString();
        var token = "test-token";

        _oAuthTokenServiceMock
            .Setup(x => x.GetAuthTokenAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        var sut = CreateSut(async (request, cancellationToken) =>
        {
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        var result = await sut.GetCustomerByIdAsync(customerId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCustomerByIdAsync_ConErrorHttp_ThrowsException()
    {
        var customerId = Guid.NewGuid().ToString();
        var token = "test-token";

        _oAuthTokenServiceMock
            .Setup(x => x.GetAuthTokenAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        var sut = CreateSut(async (request, cancellationToken) =>
        {
            return new HttpResponseMessage(HttpStatusCode.InternalServerError);
        });

        var act = () => sut.GetCustomerByIdAsync(customerId);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage($"Error al realizar la solicitud HTTP para obtener el cliente con ID {customerId}*");
    }

    [Fact]
    public async Task GetCustomerByIdAsync_ConSolicitudCancelada_ThrowsOperationCanceledException()
    {
        var customerId = Guid.NewGuid().ToString();
        var token = "test-token";

        _oAuthTokenServiceMock
            .Setup(x => x.GetAuthTokenAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        var sut = CreateSut(async (request, cancellationToken) =>
        {
            throw new TaskCanceledException();
        });

        var act = () => sut.GetCustomerByIdAsync(customerId);

        await act.Should().ThrowAsync<OperationCanceledException>()
            .WithMessage($"La solicitud para obtener el cliente con ID {customerId} fue cancelada*");
    }

    [Fact]
    public async Task GetCustomerByIdAsync_ConRespuestaNull_ThrowsException()
    {
        var customerId = Guid.NewGuid().ToString();
        var token = "test-token";

        _oAuthTokenServiceMock
            .Setup(x => x.GetAuthTokenAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        var sut = CreateSut(async (request, cancellationToken) =>
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create<CustomerResponse>(null!)
            };
        });

        var act = () => sut.GetCustomerByIdAsync(customerId);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage($"Error inesperado al obtener el cliente con ID {customerId}*");
    }

    [Fact]
    public async Task GetCustomerByIdAsync_ConErrorInesperado_ThrowsException()
    {
        var customerId = Guid.NewGuid().ToString();
        var token = "test-token";

        _oAuthTokenServiceMock
            .Setup(x => x.GetAuthTokenAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        var sut = CreateSut(async (request, cancellationToken) =>
        {
            throw new InvalidOperationException("Unexpected error");
        });

        var act = () => sut.GetCustomerByIdAsync(customerId);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage($"Error inesperado al obtener el cliente con ID {customerId}*");
    }
}