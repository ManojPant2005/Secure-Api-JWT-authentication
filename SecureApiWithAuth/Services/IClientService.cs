using SecureApiWithAuth.Data.Entity;

namespace SecureApiWithAuth.Services
{
    public interface IClientService
    {
        Task<(bool flag, string Message)> RegisterUserAsync(Registration model);
        Task<(bool flag, string Token)> LoginUserAsync(Login model);
    }
}
