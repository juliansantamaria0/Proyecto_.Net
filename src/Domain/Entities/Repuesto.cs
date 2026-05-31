using AutoTallerManager.Domain.Common;
using AutoTallerManager.Domain.Exceptions;

namespace AutoTallerManager.Domain.Entities;

public class Repuesto : BaseEntity
{
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public int CantidadStock { get; set; }
    public int StockMinimo { get; set; } = 10;
    public decimal PrecioUnitario { get; set; }
    public bool Activo { get; set; } = true;

    public ICollection<DetalleOrden> DetallesOrden { get; set; } = new List<DetalleOrden>();

    public void ValidarDisponibilidad(int cantidad, int cantidadComprometidaEnOrdenesActivas = 0)
    {
        if (cantidad <= 0)
            throw new BusinessRuleException("La cantidad debe ser mayor a cero.");

        if (!Activo)
            throw new BusinessRuleException($"El repuesto {Codigo} no está activo.");

        var disponible = CantidadStock - cantidadComprometidaEnOrdenesActivas;
        if (disponible < cantidad)
            throw new BusinessRuleException(
                $"Stock insuficiente para el repuesto {Codigo}. Disponible: {disponible}, solicitado: {cantidad}.");
    }

    public void Descontar(int cantidad)
    {
        ValidarDisponibilidad(cantidad);
        CantidadStock -= cantidad;
        UpdatedAt = DateTime.UtcNow;
    }
}
