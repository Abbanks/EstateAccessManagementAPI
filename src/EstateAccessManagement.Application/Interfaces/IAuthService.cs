using EstateAccessManagement.Core.Entities;

namespace EstateAccessManagement.Application.Interfaces
{
    public interface IAuthService
    {
        Task<string> GenerateJwtToken(AppUser user);
    }
}
