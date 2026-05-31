using AutoTallerManager.Domain.Entities;
using AutoTallerManager.Domain.Ports.Output;
using AutoTallerManager.Infrastructure.Persistence;

namespace AutoTallerManager.Infrastructure.Repositories;

public class ClienteRepository(AutoTallerDbContext context) : GenericRepository<Cliente>(context), IClienteRepository;

public class VehiculoRepository(AutoTallerDbContext context) : GenericRepository<Vehiculo>(context), IVehiculoRepository;

public class OrdenServicioRepository(AutoTallerDbContext context) : GenericRepository<OrdenServicio>(context), IOrdenServicioRepository;

public class RepuestoRepository(AutoTallerDbContext context) : GenericRepository<Repuesto>(context), IRepuestoRepository;

public class DetalleOrdenRepository(AutoTallerDbContext context) : GenericRepository<DetalleOrden>(context), IDetalleOrdenRepository;

public class FacturaRepository(AutoTallerDbContext context) : GenericRepository<Factura>(context), IFacturaRepository;

public class UsuarioRepository(AutoTallerDbContext context) : GenericRepository<Usuario>(context), IUsuarioRepository;

public class AuditoriaRepository(AutoTallerDbContext context) : GenericRepository<Auditoria>(context), IAuditoriaRepository;
