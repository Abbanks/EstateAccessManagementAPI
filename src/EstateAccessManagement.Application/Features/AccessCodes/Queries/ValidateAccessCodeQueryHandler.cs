using EstateAccessManagement.Application.Features.AccessCodes.DTOs;
using EstateAccessManagement.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EstateAccessManagement.Application.Features.AccessCodes.Queries
{
    public class ValidateAccessCodeQueryHandler(
        ILogger<ValidateAccessCodeQueryHandler> logger,
        IAccessCodeService accessCodeService) : IRequestHandler<ValidateAccessCodeQuery, AccessCodeValidationResult>
    {
        public async Task<AccessCodeValidationResult> Handle(ValidateAccessCodeQuery request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Validating access code.");

            var validationResult = await accessCodeService.ValidateAccessCodeAsync(request.Code);

            return new AccessCodeValidationResult
            {
                IsValid = validationResult.IsValid,
                Message = validationResult.Message,
                AccessCodeId = validationResult.AccessCodeId ?? Guid.Empty,
                ResidentId = validationResult.ResidentId ?? Guid.Empty
            };
        }
    }
}
