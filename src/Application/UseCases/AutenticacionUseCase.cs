using AutoMapper;
using AutoTallerManager.Application.DTOs;
using AutoTallerManager.Application.Ports.Input;
using AutoTallerManager.Application.Ports.Output;
using AutoTallerManager.Domain.Entities;
using AutoTallerManager.Domain.Enums;
using AutoTallerManager.Domain.Exceptions;
using AutoTallerManager.Domain.Ports.Output;

namespace AutoTallerManager.Application.UseCases;

public class AutenticacionUseCase(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IJwtTokenProvider jwtTokenProvider,
    IPasswordHasher passwordHasher,
    IAuditoriaRegistroPort auditoriaRegistro) : IAutenticacionUseCase
{
    public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
    {
        var usuarios = await unitOfWork.Usuarios.FindAsync(u => u.Correo == dto.Correo);
        var usuario = usuarios.FirstOrDefault();

        if (usuario is null || !usuario.Activo || !passwordHasher.Verify(dto.Password, usuario.PasswordHash))
            throw new BusinessRuleException("Credenciales inválidas.");

        return BuildLoginResponse(usuario);
    }

    public async Task<LoginResponseDto> RegisterAsync(RegisterDto dto)
    {
        var correo = dto.Correo.Trim().ToLowerInvariant();

        if (await unitOfWork.Usuarios.ExistsAsync(u => u.Correo == correo))
            throw new BusinessRuleException("Ya existe una cuenta con ese correo electrónico.");

        if (await unitOfWork.Clientes.ExistsAsync(c => c.Correo == correo))
            throw new BusinessRuleException("El correo ya está registrado como cliente del taller.");

        Usuario usuario = null!;

        await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var cliente = new Cliente
            {
                Nombre = dto.Nombre.Trim(),
                Telefono = dto.Telefono.Trim(),
                Correo = correo,
            };

            usuario = new Usuario
            {
                Nombre = dto.Nombre.Trim(),
                Correo = correo,
                PasswordHash = passwordHasher.Hash(dto.Password),
                Rol = RolUsuario.Cliente,
                Activo = true,
                Cliente = cliente,
            };

            await unitOfWork.Usuarios.AddAsync(usuario);
            await unitOfWork.CommitAsync();

            await auditoriaRegistro.RegistrarAsync(nameof(Cliente), cliente.Id, TipoAccionAuditoria.Crear, usuario.Id,
                "Registro público de cliente.");
            await auditoriaRegistro.RegistrarAsync(nameof(Usuario), usuario.Id, TipoAccionAuditoria.Crear, usuario.Id,
                "Registro público con rol Cliente.");
            await unitOfWork.CommitAsync();
        });

        return BuildLoginResponse(usuario);
    }

    private LoginResponseDto BuildLoginResponse(Usuario usuario)
    {
        var expiration = DateTime.UtcNow.AddHours(jwtTokenProvider.GetExpirationHours());

        return new LoginResponseDto
        {
            Token = jwtTokenProvider.GenerateToken(usuario, expiration),
            Expiration = expiration,
            Usuario = mapper.Map<UsuarioDto>(usuario),
        };
    }
}
