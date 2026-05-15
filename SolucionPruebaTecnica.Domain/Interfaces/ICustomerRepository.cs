using SolucionPruebaTecnica.Domain.Entities;

namespace SolucionPruebaTecnica.Domain.Interfaces;

public interface ICustomerRepository
{
    public Task<Customer?> GetCustomerByIdAsync(string id, CancellationToken cancellationToken = default);
}