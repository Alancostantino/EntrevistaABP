using AutoMapper;
using WB.EntrevistaABP.Application.Contracts.Dtos;
using WB.EntrevistaABP.Domain.Entidades;

namespace WB.EntrevistaABP;

public class EntrevistaABPApplicationAutoMapperProfile : Profile
{
    public EntrevistaABPApplicationAutoMapperProfile()
    {
         CreateMap<Pasajero, PasajeroDto>();

        CreateMap<Viaje, ViajeDto>()
            .ForMember(d => d.Coordinador, m => m.MapFrom(s => s.Coordinador))
            .ForMember(d => d.Pasajeros,  m => m.MapFrom(s => s.Pasajeros));
    }
}
