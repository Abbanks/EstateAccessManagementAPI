using EstateAccessManagement.Application.Features.Users;
using EstateAccessManagement.Application.Interfaces.Services;

namespace EstateAccessManagement.Infrastructure.Services;
public class UserService(
   ApplicationDbContext db
   ) : IUserService
{
    public async Task<GetUserByIdResult> GetUserById(Guid id)
    {
        var user = await db.AppUsers.FindAsync(id);

        if (user == null)
        {
            return null;
        } 

        return new GetUserByIdResult
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            UserType = user.UserType,
            CreatedAt = user.CreatedAt,
            IsDeprecated = user.IsDeprecated
        };
    }
}