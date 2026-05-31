using AutoMapper;
using AutoTallerManager.Application.DTOs;
using AutoTallerManager.Domain.Entities;
using AutoTallerManager.Domain.Enums;

namespace AutoTallerManager.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Cliente, ClienteDto>()
            .ForMember(d => d.CantidadVehiculos, o => o.MapFrom(s => s.Vehiculos.Count));
        CreateMap<CreateClienteDto, Cliente>();
        CreateMap<UpdateClienteDto, Cliente>();

        CreateMap<Vehiculo, VehiculoDto>()
            .ForMember(d => d.ClienteNombre, o => o.MapFrom(s => s.Cliente.Nombre));
        CreateMap<CreateVehiculoDto, Vehiculo>();
        CreateMap<VehiculoRegistroDto, Vehiculo>();
        CreateMap<UpdateVehiculoDto, Vehiculo>();

        CreateMap<OrdenServicio, OrdenServicioDto>()
            .ForMember(d => d.ClienteId, o => o.MapFrom(s => s.Vehiculo.ClienteId))
            .ForMember(d => d.VehiculoDescripcion, o => o.MapFrom(s => $"{s.Vehiculo.Marca} {s.Vehiculo.Modelo} ({s.Vehiculo.Vin})"))
            .ForMember(d => d.ClienteNombre, o => o.MapFrom(s => s.Vehiculo.Cliente.Nombre))
            .ForMember(d => d.MecanicoNombre, o => o.MapFrom(s => s.Mecanico != null ? s.Mecanico.Nombre : null));

        CreateMap<Repuesto, RepuestoDto>();
        CreateMap<CreateRepuestoDto, Repuesto>();
        CreateMap<UpdateRepuestoDto, Repuesto>();

        CreateMap<DetalleOrden, DetalleOrdenDto>()
            .ForMember(d => d.RepuestoDescripcion, o => o.MapFrom(s => s.Repuesto.Descripcion))
            .ForMember(d => d.Subtotal, o => o.MapFrom(s => s.Subtotal));

        CreateMap<Factura, FacturaDto>()
            .ForMember(d => d.ClienteId, o => o.MapFrom(s => s.OrdenServicio.Vehiculo.ClienteId))
            .ForMember(d => d.ClienteNombre, o => o.MapFrom(s => s.OrdenServicio.Vehiculo.Cliente.Nombre))
            .ForMember(d => d.VehiculoDescripcion, o => o.MapFrom(s => $"{s.OrdenServicio.Vehiculo.Marca} {s.OrdenServicio.Vehiculo.Modelo}"));

        CreateMap<Usuario, UsuarioDto>();
        CreateMap<CreateUsuarioDto, Usuario>()
            .ForMember(d => d.PasswordHash, o => o.Ignore());

        CreateMap<Auditoria, AuditoriaDto>()
            .ForMember(d => d.UsuarioNombre, o => o.MapFrom(s => s.Usuario.Nombre));
    }
}
