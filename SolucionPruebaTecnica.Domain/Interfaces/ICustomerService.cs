using SolucionPruebaTecnica.Domain.Entities;

namespace SolucionPruebaTecnica.Domain.Interfaces;

public interface ICustomerService
{
    public Task<Customer> GetCustomerByIdAsync(string id, CancellationToken cancellationToken = default);

}
