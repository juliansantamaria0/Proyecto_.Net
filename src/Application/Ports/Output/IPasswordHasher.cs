namespace AutoTallerManager.Application.Ports.Output;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
