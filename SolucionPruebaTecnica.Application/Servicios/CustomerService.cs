using SolucionPruebaTecnica.Domain.Entities;
using SolucionPruebaTecnica.Domain.Interfaces;

namespace SolucionPruebaTecnica.Application.Servicios;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public Task<Customer> GetCustomerByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("El ID del cliente no puede ser nulo o vacío.", nameof(id));

        if (!Guid.TryParse(id, out var guid))
            throw new ArgumentException("El ID del cliente debe ser un GUID válido.", nameof(id));
        return _customerRepository.GetCustomerByIdAsync(id, cancellationToken);
    }

}