using AutoTallerManager.Application.DTOs;
using AutoTallerManager.Application.Ports.Input;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoTallerManager.API.Controllers;

[ApiController]
[Route("api/usuarios")]
public class RegistroPublicoController(IAutenticacionUseCase autenticacionUseCase) : ControllerBase
{
    [HttpPost("registrar")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<LoginResponseDto>> Registrar([FromBody] RegisterDto dto) =>
        Ok(await autenticacionUseCase.RegisterAsync(dto));
}
