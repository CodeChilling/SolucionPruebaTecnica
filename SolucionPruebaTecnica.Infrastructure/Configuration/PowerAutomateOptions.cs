namespace SolucionPruebaTecnica.Infrastructure.Configuration;

public class PowerAutomateOptions
{
    public string BaseUrl { get; set; } = default!;
    public string ClientId { get; set; } = default!;
    public string ClientSecret { get; set; } = default!;
    public string TenantId { get; set; } = default!;
    public string Scope { get; set; } = default!;
    public string AuthUrl { get; set; } = default!;
}