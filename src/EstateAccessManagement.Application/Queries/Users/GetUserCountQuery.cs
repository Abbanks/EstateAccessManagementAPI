using MediatR;

namespace EstateAccessManagement.Application.Queries.Users
{
    public class GetUserCountQuery : IRequest<int>
    {
    }
}