using EstateAccessManagement.Application.Features.AccessCodes.DTOs;
using MediatR;

namespace EstateAccessManagement.Application.Features.AccessCodes.Queries
{
    public class ValidateAccessCodeQuery : IRequest<AccessCodeValidationResult>
    {
        public string Code { get; set; }
    }
}
