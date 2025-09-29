using EstateAccessManagement.Core.Entities;

namespace EstateAccessManagement.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<string> GenerateJwtToken(AppUser user);
    }
}
