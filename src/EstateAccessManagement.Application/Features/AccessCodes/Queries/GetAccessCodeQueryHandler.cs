using EstateAccessManagement.Application.Features.AccessCodes.DTOs;
using EstateAccessManagement.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EstateAccessManagement.Application.Features.AccessCodes.Queries
{
    public class GetAccessCodeQueryHandler(
      ILogger<GetAccessCodeQueryHandler> logger,
      IAccessCodeService accessCodeService) : IRequestHandler<GetAccessCodeQuery, GetAccessCodeResult>
    {
        public async Task<GetAccessCodeResult> Handle(GetAccessCodeQuery request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Fetching access code for Id: {Id}", request.Id);

            var result = await accessCodeService.GetAccessCodeByIdAsync(request.Id);

            if (result == null)
            {
                logger.LogWarning("No access code found for Id: {Id}", request.Id);
                return null;
            }

            return new GetAccessCodeResult
            {
                Id = result.Id,
                ResidentId = result.ResidentId,
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
