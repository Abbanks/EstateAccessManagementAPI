using EstateAccessManagement.Application.Queries.Users;
using EstateAccessManagement.Core.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EstateAccessManagement.Application.Handlers
{
    public class GetUserCountHandler(UserManager<AppUser> userManager) 
        : IRequestHandler<GetUserCountQuery, int>
    {
        public async Task<int> Handle(GetUserCountQuery request, CancellationToken cancellationToken)
        {
            return await userManager.Users.CountAsync(cancellationToken);
        }
    }
}