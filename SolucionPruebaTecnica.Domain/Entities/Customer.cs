namespace SolucionPruebaTecnica.Domain.Entities;

public class Customer
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public int Age { get; set; } = default!;
    public string City { get; set; } = default!;
}
