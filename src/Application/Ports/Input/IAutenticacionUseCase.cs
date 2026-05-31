using AutoTallerManager.Application.DTOs;

namespace AutoTallerManager.Application.Ports.Input;

public interface IAutenticacionUseCase
{
    Task<LoginResponseDto> LoginAsync(LoginDto dto);
    Task<LoginResponseDto> RegisterAsync(RegisterDto dto);
}
