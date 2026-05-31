namespace AutoTallerManager.Domain.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string entity, int id)
        : base($"{entity} con id {id} no fue encontrado.") { }

    public NotFoundException(string message) : base(message) { }
}
