namespace AutoTallerManager.Domain.Ports.Output;

public interface IUnitOfWork : IDisposable
{
    IClienteRepository Clientes { get; }
    IVehiculoRepository Vehiculos { get; }
    IOrdenServicioRepository OrdenesServicio { get; }
    IRepuestoRepository Repuestos { get; }
    IDetalleOrdenRepository DetalleOrdenes { get; }
    IFacturaRepository Facturas { get; }
    IUsuarioRepository Usuarios { get; }
    IAuditoriaRepository Auditorias { get; }

    Task<int> CommitAsync(CancellationToken cancellationToken = default);

    Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default);
}
