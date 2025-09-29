using EstateAccessManagement.Application.Features.Users;

namespace EstateAccessManagement.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<GetUserByIdResult> GetUserById(Guid id);
    }
}
