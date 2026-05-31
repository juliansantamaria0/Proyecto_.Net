namespace AutoTallerManager.Application.DTOs;

public class ClienteDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public int CantidadVehiculos { get; set; }
}

public class CreateClienteDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
}

public class UpdateClienteDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
}

public class RegistrarClienteConVehiculoDto
{
    public CreateClienteDto Cliente { get; set; } = new();
    public List<VehiculoRegistroDto> Vehiculos { get; set; } = [];
}

public class VehiculoRegistroDto
{
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int Anio { get; set; }
    public string Vin { get; set; } = string.Empty;
    public int Kilometraje { get; set; }
}
