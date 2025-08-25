using EstateAccessManagement.Core.Entities;

namespace EstateAccessManagement.Application.Services
{
    public interface IAuthService
    {
        Task<string> GenerateJwtToken(AppUser user);
    }
}
