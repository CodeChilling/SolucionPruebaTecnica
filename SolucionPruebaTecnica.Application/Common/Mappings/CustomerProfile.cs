using AutoMapper;
using SolucionPruebaTecnica.Application.Common.DTO;
using SolucionPruebaTecnica.Domain.Entities;

namespace SolucionPruebaTecnica.Application.Common.Mappings;

public class CustomerProfile : Profile
{
    public CustomerProfile()
    {
        CreateMap<Customer, CustomerResponse>();
    }
}