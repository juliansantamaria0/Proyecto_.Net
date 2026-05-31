using AutoTallerManager.Application.DTOs;
using AutoTallerManager.Application.Ports.Input;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoTallerManager.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAutenticacionUseCase autenticacionUseCase) : ControllerBase
{
    /// <summary>
    /// Autentica un usuario y devuelve un token JWT Bearer.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto dto)
    {
        var result = await autenticacionUseCase.LoginAsync(dto);
        return Ok(result);
    }

    /// <summary>
    /// Registro público de clientes (rol Cliente). Crea perfil de cliente y usuario vinculado.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<LoginResponseDto>> Register([FromBody] RegisterDto dto)
    {
        var result = await autenticacionUseCase.RegisterAsync(dto);
        return Ok(result);
    }
}
