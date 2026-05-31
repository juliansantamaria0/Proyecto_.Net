namespace AutoTallerManager.Application.DTOs;

public class RepuestoDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public int CantidadStock { get; set; }
    public int StockMinimo { get; set; }
    public decimal PrecioUnitario { get; set; }
    public bool Activo { get; set; }
}

public class CreateRepuestoDto
{
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public int CantidadStock { get; set; }
    public int StockMinimo { get; set; } = 10;
    public decimal PrecioUnitario { get; set; }
}

public class UpdateRepuestoDto
{
    public string Descripcion { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public int CantidadStock { get; set; }
    public int StockMinimo { get; set; } = 10;
    public decimal PrecioUnitario { get; set; }
    public bool Activo { get; set; }
}

public class UpdateStockDto
{
    public int CantidadStock { get; set; }
}
