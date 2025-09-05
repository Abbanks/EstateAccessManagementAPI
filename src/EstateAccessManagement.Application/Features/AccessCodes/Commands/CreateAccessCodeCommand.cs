using EstateAccessManagement.Application.Features.AccessCodes.DTOs;
using EstateAccessManagement.Core.Enums;
using MediatR;

namespace EstateAccessManagement.Application.Features.AccessCodes.Commands
{
    public class CreateAccessCodeCommand : IRequest<CreateAccessCodeResult>
    {
        public AccessCodeType CodeType { get; set; }
    }
}
