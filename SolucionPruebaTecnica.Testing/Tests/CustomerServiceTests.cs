using FluentAssertions;
using Moq;
using SolucionPruebaTecnica.Application.Servicios;
using SolucionPruebaTecnica.Domain.Entities;
using SolucionPruebaTecnica.Domain.Interfaces;

namespace SolucionPruebaTecnica.Testing.Tests;

public class CustomerServiceTests
{
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly CustomerService _sut;

    public CustomerServiceTests()
    {
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _sut = new CustomerService(_customerRepositoryMock.Object);
    }

    [Fact]
    public async Task GetCustomerByIdAsync_ConIdValido_RetornaCliente()
    {
        var customerId = Guid.NewGuid().ToString();
        var expectedCustomer = new Customer
        {
            Id = Guid.Parse(customerId),
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Age = 30,
            City = "New York"
        };

        _customerRepositoryMock
            .Setup(x => x.GetCustomerByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCustomer);

        var result = await _sut.GetCustomerByIdAsync(customerId);

        result.Should().NotBeNull();
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Email.Should().Be("john.doe@example.com");
        result.Age.Should().Be(30);
        result.City.Should().Be("New York");

        _customerRepositoryMock.Verify(
            x => x.GetCustomerByIdAsync(customerId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetCustomerByIdAsync_ConIdNulo_ThrowsArgumentException()
    {
        var act = () => _sut.GetCustomerByIdAsync(null!);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("El ID del cliente no puede ser nulo o vacío.*");
    }

    [Fact]
    public async Task GetCustomerByIdAsync_ConIdVacio_ThrowsArgumentException()
    {
        var act = () => _sut.GetCustomerByIdAsync("");

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("El ID del cliente no puede ser nulo o vacío.*");
    }

    [Fact]
    public async Task GetCustomerByIdAsync_ConIdEspacios_ThrowsArgumentException()
    {
        var act = () => _sut.GetCustomerByIdAsync("   ");

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("El ID del cliente debe ser un GUID válido.*");
    }

    [Theory]
    [InlineData("invalid-guid")]
    [InlineData("12345")]
    [InlineData("not-a-guid")]
    public async Task GetCustomerByIdAsync_ConIdInvalido_ThrowsArgumentException(string invalidId)
    {
        var act = () => _sut.GetCustomerByIdAsync(invalidId);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("El ID del cliente debe ser un GUID válido.*");
    }

    [Fact]
    public async Task GetCustomerByIdAsync_ConIdValidoPeroNoExiste_ThrowsException()
    {
        var customerId = Guid.NewGuid().ToString();

        _customerRepositoryMock
            .Setup(x => x.GetCustomerByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Customer not found"));

        var act = () => _sut.GetCustomerByIdAsync(customerId);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Customer not found");
    }
}