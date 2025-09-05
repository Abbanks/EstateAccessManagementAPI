using EstateAccessManagement.Application.Features.AccessCodes.DTOs;
using EstateAccessManagement.Application.Interfaces;
using EstateAccessManagement.Core.Enums;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace EstateAccessManagement.Application.Features.AccessCodes.Commands
{
    public class CreateAccessCodeCommandHandler(ILogger<CreateAccessCodeCommandHandler> logger, IAccessCodeService accessCodeService,
         IHttpContextAccessor httpContextAccessor) : IRequestHandler<CreateAccessCodeCommand, CreateAccessCodeResult>
    {
        public async Task<CreateAccessCodeResult> Handle(CreateAccessCodeCommand request, CancellationToken cancellationToken)
        {
            var userIdClaim = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var residentId))
            {
                throw new UnauthorizedAccessException("Invalid user token");
            }

            if (!Enum.TryParse(typeof(AccessCodeType), request.CodeType.ToString(), true, out var parsedCodeType))
            {
                throw new ValidationException("Invalid code type.");
            }

            var result = await accessCodeService.GenerateAccessCodeAsync(residentId, (AccessCodeType)parsedCodeType);

            return new CreateAccessCodeResult
            {
                Id = result.Id,
                ResidentId = result.ResidentId,
                Code = result.Code,
                CodeType = result.CodeType,
                ExpiresAt = result.ExpiresAt,
                MaxUses = result.MaxUses,
                CurrentUses = result.CurrentUses,
                IsActive = result.IsActive,
                CreatedAt = result.CreatedAt
            };
        }
    }
}
