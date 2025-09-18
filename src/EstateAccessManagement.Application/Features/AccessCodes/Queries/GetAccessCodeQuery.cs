using EstateAccessManagement.Application.Features.AccessCodes.DTOs;
using MediatR;

namespace EstateAccessManagement.Application.Features.AccessCodes.Queries
{
    public class GetAccessCodeQuery : IRequest<GetAccessCodeResult>
    {
        public Guid Id { get; set; }
    }
}
