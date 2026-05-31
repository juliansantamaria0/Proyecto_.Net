using AutoTallerManager.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace AutoTallerManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected int GetCurrentUserId()
    {
        var claim = User.FindFirst("UserId") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
    }

    protected int GetCurrentClienteId()
    {
        var claim = User.FindFirst("ClienteId");
        return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
    }

    protected bool IsClienteRole() => User.IsInRole("Cliente");

    protected bool IsRecepcionistaRole() => User.IsInRole("Recepcionista");

    protected bool IsMecanicoRole() => User.IsInRole("Mecanico");

    protected int? ResolveMecanicoIdFilter(int? requestedMecanicoId)
    {
        if (!IsMecanicoRole()) return requestedMecanicoId;
        return GetCurrentUserId();
    }

    protected ActionResult? ForbidIfNotAssignedMecanico(int? ordenMecanicoId)
    {
        if (!IsMecanicoRole()) return null;
        var userId = GetCurrentUserId();
        if (ordenMecanicoId != userId)
            return Forbid();
        return null;
    }

    protected int? ResolveClienteIdFilter(int? requestedClienteId)
    {
        if (!IsClienteRole()) return requestedClienteId;

        var clienteId = GetCurrentClienteId();
        if (clienteId <= 0)
            throw new UnauthorizedAccessException("El usuario cliente no tiene un perfil de cliente vinculado.");

        if (requestedClienteId.HasValue && requestedClienteId.Value != clienteId)
            throw new UnauthorizedAccessException("No puede consultar datos de otro cliente.");

        return clienteId;
    }

    protected ActionResult? ForbidIfNotOwnCliente(int resourceClienteId)
    {
        if (IsClienteRole() && GetCurrentClienteId() != resourceClienteId)
            return Forbid();
        return null;
    }

    protected void AddPaginationHeader(PagedResult<object> result)
    {
        Response.Headers["X-Total-Count"] = result.TotalCount.ToString();
        Response.Headers["Access-Control-Expose-Headers"] = "X-Total-Count";
    }

    protected void AddPaginationHeader<T>(PagedResult<T> result)
    {
        Response.Headers["X-Total-Count"] = result.TotalCount.ToString();
        Response.Headers["Access-Control-Expose-Headers"] = "X-Total-Count";
    }
}
