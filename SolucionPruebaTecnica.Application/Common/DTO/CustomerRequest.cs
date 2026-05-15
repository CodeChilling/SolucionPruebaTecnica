using System.ComponentModel.DataAnnotations;

namespace SolucionPruebaTecnica.Application.Common.DTO;

public class CustomerRequest
{
    [Required(ErrorMessage = "El campo customerId es requerido.")]
    public string customerId { get; set; } = default!;
}