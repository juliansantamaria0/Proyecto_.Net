using AutoTallerManager.Domain.Entities;

namespace AutoTallerManager.Application.Ports.Output;

public interface IJwtTokenProvider
{
    string GenerateToken(Usuario usuario, DateTime expiration);
    int GetExpirationHours();
}
