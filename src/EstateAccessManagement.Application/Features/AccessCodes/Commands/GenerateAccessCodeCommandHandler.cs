using EstateAccessManagement.Application.Features.AccessCodes.DTOs;
using EstateAccessManagement.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace EstateAccessManagement.Application.Features.AccessCodes.Commands
{
    public class GenerateAccessCodeCommandHandler(
         ILogger<GenerateAccessCodeCommandHandler> logger,
         IAccessCodeService accessCodeService,
         IHttpContextAccessor httpContextAccessor) : IRequestHandler<GenerateAccessCodeCommand, GenerateAccessCodeResult>
    {
        public async Task<GenerateAccessCodeResult> Handle(GenerateAccessCodeCommand request, CancellationToken cancellationToken)
        {
            var userIdClaim = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var residentId))
            {
                throw new UnauthorizedAccessException("Invalid user token");
            }

            logger.LogInformation("Generating {CodeType} access code for resident {ResidentId}", request.CodeType, residentId);
            var result = await accessCodeService.GenerateAccessCodeAsync(residentId, request.CodeType);

            return new GenerateAccessCodeResult
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
