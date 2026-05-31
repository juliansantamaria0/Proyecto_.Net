using AutoTallerManager.Domain.Entities;
using AutoTallerManager.Domain.Ports.Output;
using AutoTallerManager.Infrastructure.Persistence;

namespace AutoTallerManager.Infrastructure.Repositories;

public class UnitOfWork(AutoTallerDbContext context) : IUnitOfWork
{
    private IClienteRepository? _clientes;
    private IVehiculoRepository? _vehiculos;
    private IOrdenServicioRepository? _ordenesServicio;
    private IRepuestoRepository? _repuestos;
    private IDetalleOrdenRepository? _detalleOrdenes;
    private IFacturaRepository? _facturas;
    private IUsuarioRepository? _usuarios;
    private IAuditoriaRepository? _auditorias;

    public IClienteRepository Clientes =>
        _clientes ??= new ClienteRepository(context);

    public IVehiculoRepository Vehiculos =>
        _vehiculos ??= new VehiculoRepository(context);

    public IOrdenServicioRepository OrdenesServicio =>
        _ordenesServicio ??= new OrdenServicioRepository(context);

    public IRepuestoRepository Repuestos =>
        _repuestos ??= new RepuestoRepository(context);

    public IDetalleOrdenRepository DetalleOrdenes =>
        _detalleOrdenes ??= new DetalleOrdenRepository(context);

    public IFacturaRepository Facturas =>
        _facturas ??= new FacturaRepository(context);

    public IUsuarioRepository Usuarios =>
        _usuarios ??= new UsuarioRepository(context);

    public IAuditoriaRepository Auditorias =>
        _auditorias ??= new AuditoriaRepository(context);

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default) =>
        await context.SaveChangesAsync(cancellationToken);

    public async Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await action();
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public void Dispose() => context.Dispose();
}
