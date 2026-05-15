using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SolucionPruebaTecnica.Application.Common.DTO;
using SolucionPruebaTecnica.Domain.Entities;
using SolucionPruebaTecnica.Domain.Interfaces;
using SolucionPruebaTecnica.Infrastructure.Configuration;
using System.Net.Http.Json;

namespace SolucionPruebaTecnica.Infrastructure.Adapter;

public class PowerAutomateCustomerAdapter : ICustomerRepository
{
    private readonly IOAuthTokenService _oAuthTokenService;
    private readonly PowerAutomateOptions _powerAutomateOptions;
    private readonly HttpClient _httpClient;
    private readonly ILogger<PowerAutomateCustomerAdapter> _logger;

    public PowerAutomateCustomerAdapter(IOAuthTokenService oAuthTokenService, IOptions<PowerAutomateOptions> powerAutomateOptions, HttpClient httpClient, ILogger<PowerAutomateCustomerAdapter> logger)
    {
        _oAuthTokenService = oAuthTokenService;
        _powerAutomateOptions = powerAutomateOptions.Value;
        _httpClient = httpClient;
        _logger = logger;
    }
    public async Task<Customer?> GetCustomerByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await _oAuthTokenService.GetAuthTokenAsync(cancellationToken);
            var request = new { customerId = id };
            var requestContent = new HttpRequestMessage(HttpMethod.Post, _powerAutomateOptions.BaseUrl)
            {
                Content = JsonContent.Create(request)
            };
            requestContent.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.SendAsync(requestContent, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Cliente con ID {CustomerId} no encontrado", id);

                return null;
            }
            response.EnsureSuccessStatusCode();
            var customer = await response.Content.ReadFromJsonAsync<CustomerResponse>(cancellationToken: cancellationToken);
            if (customer == null)
            {
                throw new Exception($"Respuesta inválida al obtener el cliente con ID {id}");
            }
            return new Customer
            {
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                Age = customer.Age,
                City = customer.City
            };
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "La solicitud para obtener el cliente con ID {CustomerId} fue cancelada", id);
            throw new OperationCanceledException($"La solicitud para obtener el cliente con ID {id} fue cancelada", ex, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error al realizar la solicitud HTTP para obtener el cliente con ID {CustomerId}", id);
            throw new Exception($"Error al realizar la solicitud HTTP para obtener el cliente con ID {id}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado al obtener el cliente con ID {CustomerId}", id);
            throw new Exception($"Error inesperado al obtener el cliente con ID {id}", ex);
        }
    }

}
