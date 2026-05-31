using AutoMapper;
using AutoTallerManager.Application.Common;
using AutoTallerManager.Application.DTOs;
using AutoTallerManager.Application.Ports.Input;
using AutoTallerManager.Application.Ports.Output;
using AutoTallerManager.Domain.Entities;
using AutoTallerManager.Domain.Enums;
using AutoTallerManager.Domain.Exceptions;
using AutoTallerManager.Domain.Ports.Output;

namespace AutoTallerManager.Application.UseCases;

public class GestionarUsuariosUseCase(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IPasswordHasher passwordHasher,
    IAuditoriaRegistroPort auditoriaRegistro) : IGestionarUsuariosUseCase
{
    public async Task<PagedResult<UsuarioDto>> GetPagedAsync(PaginationParams pagination)
    {
        var (items, total) = await unitOfWork.Usuarios.GetPagedAsync(
            pagination.PageNumber,
            pagination.PageSize,
            orderBy: u => u.Nombre);

        return new PagedResult<UsuarioDto>
        {
            Items = mapper.Map<IReadOnlyList<UsuarioDto>>(items),
            TotalCount = total,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize
        };
    }

    public async Task<UsuarioDto> GetByIdAsync(int id)
    {
        var usuario = await unitOfWork.Usuarios.GetByIdAsync(id)
            ?? throw new NotFoundException("Usuario", id);
        return mapper.Map<UsuarioDto>(usuario);
    }

    public async Task<UsuarioDto> CreateAsync(CreateUsuarioDto dto, int usuarioId)
    {
        if (await unitOfWork.Usuarios.ExistsAsync(u => u.Correo == dto.Correo))
            throw new BusinessRuleException($"El correo {dto.Correo} ya está registrado.");

        var usuario = mapper.Map<Usuario>(dto);
        usuario.PasswordHash = passwordHasher.Hash(dto.Password);
        await unitOfWork.Usuarios.AddAsync(usuario);
        await unitOfWork.CommitAsync();
        await auditoriaRegistro.RegistrarAsync(nameof(Usuario), usuario.Id, TipoAccionAuditoria.Crear, usuarioId);
        await unitOfWork.CommitAsync();
        return mapper.Map<UsuarioDto>(usuario);
    }

    public async Task<UsuarioDto> UpdateAsync(int id, UpdateUsuarioDto dto, int usuarioId)
    {
        var usuario = await unitOfWork.Usuarios.GetByIdAsync(id)
            ?? throw new NotFoundException("Usuario", id);

        if (await unitOfWork.Usuarios.ExistsAsync(u => u.Correo == dto.Correo && u.Id != id))
            throw new BusinessRuleException($"El correo {dto.Correo} ya está registrado.");

        mapper.Map(dto, usuario);
        usuario.UpdatedAt = DateTime.UtcNow;
        unitOfWork.Usuarios.Update(usuario);
        await auditoriaRegistro.RegistrarAsync(nameof(Usuario), id, TipoAccionAuditoria.Modificar, usuarioId);
        await unitOfWork.CommitAsync();
        return mapper.Map<UsuarioDto>(usuario);
    }

    public async Task DeleteAsync(int id, int usuarioId)
    {
        var usuario = await unitOfWork.Usuarios.GetByIdAsync(id)
            ?? throw new NotFoundException("Usuario", id);

        if (usuario.Id == usuarioId)
            throw new BusinessRuleException("No puede eliminar su propio usuario.");

        usuario.Activo = false;
        usuario.UpdatedAt = DateTime.UtcNow;
        unitOfWork.Usuarios.Update(usuario);
        await auditoriaRegistro.RegistrarAsync(nameof(Usuario), id, TipoAccionAuditoria.Eliminar, usuarioId);
        await unitOfWork.CommitAsync();
    }
}
