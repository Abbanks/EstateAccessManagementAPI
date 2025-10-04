using EstateAccessManagement.Application.Features.AccessCodes.DTOs;
using EstateAccessManagement.Application.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace EstateAccessManagement.Application.Features.AccessCodes.Queries
{
    public class ValidateAccessCodeQueryHandler(
        ILogger<ValidateAccessCodeQueryHandler> logger,
        IAccessCodeService accessCodeService,
        IHttpContextAccessor httpContextAccessor) : IRequestHandler<ValidateAccessCodeQuery, AccessCodeValidationResult>
    {
        public async Task<AccessCodeValidationResult> Handle(ValidateAccessCodeQuery request, CancellationToken cancellationToken)
        {
            var userIdClaim = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var securityId))
            {
                throw new UnauthorizedAccessException("Invalid user token");
            }

            logger.LogInformation("Validating access code");

            var validationResult = await accessCodeService.ValidateAccessCodeAsync(request.Code, securityId);

            return new AccessCodeValidationResult
            {
                IsValid = validationResult.IsValid,
                Message = validationResult.Message,
                AccessCodeId = validationResult.AccessCodeId ?? null,
                ResidentId = validationResult.ResidentId ?? null
            };
        }
    }
}
