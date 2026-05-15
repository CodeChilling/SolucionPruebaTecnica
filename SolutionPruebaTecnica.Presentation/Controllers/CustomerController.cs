using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SolucionPruebaTecnica.Application.Common.DTO;
using SolucionPruebaTecnica.Domain.Interfaces;

namespace SolutionPruebaTecnica.Presentation.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CustomerController : Controller
{
    private readonly ICustomerService _customerService;
    private readonly IMapper _mapper;

    public CustomerController(ICustomerService customerService, IMapper mapper)
    {
        _customerService = customerService;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<ActionResult<CustomerResponse>> Consult([FromBody] CustomerRequest request, CancellationToken cancellationToken)
    {
        var customer = await _customerService.GetCustomerByIdAsync(request.customerId, cancellationToken);
        if (customer == null)
        {
            return NotFound(new { message = "Customer not found" });
        }
        var response = _mapper.Map<CustomerResponse>(customer);
        return Ok(response);
    }
}