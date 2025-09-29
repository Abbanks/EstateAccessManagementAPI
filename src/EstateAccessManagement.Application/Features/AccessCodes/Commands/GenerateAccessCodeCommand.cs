using EstateAccessManagement.Application.Features.AccessCodes.DTOs;
using EstateAccessManagement.Core.Enums;
using MediatR;

namespace EstateAccessManagement.Application.Features.AccessCodes.Commands
{
    public class GenerateAccessCodeCommand : IRequest<GenerateAccessCodeResult>
    {
        public AccessCodeType CodeType { get; set; }
    }
}
