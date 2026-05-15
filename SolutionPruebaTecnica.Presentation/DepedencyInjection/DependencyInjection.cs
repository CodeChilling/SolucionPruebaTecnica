using Polly;
using Polly.Extensions.Http;
using SolucionPruebaTecnica.Application.Servicios;
using SolucionPruebaTecnica.Domain.Interfaces;
using SolucionPruebaTecnica.Infrastructure.Adapter;
using SolucionPruebaTecnica.Infrastructure.Authentication;
using SolucionPruebaTecnica.Infrastructure.Configuration;

namespace SolutionPruebaTecnica.Presentation.DepedencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICustomerService, CustomerService>();
        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PowerAutomateOptions>(configuration.GetSection("PowerAutomate"));

        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        var circuitBreakerPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

        services.AddHttpClient<IOAuthTokenService, OAuthTokenService>().
            ConfigureHttpClient(client => client.Timeout = TimeSpan.FromSeconds(30))
            .AddPolicyHandler(retryPolicy)
            .AddPolicyHandler(circuitBreakerPolicy);

        services.AddHttpClient<ICustomerRepository, PowerAutomateCustomerAdapter>().
            ConfigureHttpClient(client => client.Timeout = TimeSpan.FromSeconds(30))
            .AddPolicyHandler(retryPolicy)
            .AddPolicyHandler(circuitBreakerPolicy);

        return services;
    }
}