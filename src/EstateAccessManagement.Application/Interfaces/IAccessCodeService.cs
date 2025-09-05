using EstateAccessManagement.Application.Features.AccessCodes.DTOs;
using EstateAccessManagement.Core.Enums;

namespace EstateAccessManagement.Application.Interfaces
{
    public interface IAccessCodeService
    {
        Task<CreateAccessCodeResult> GenerateAccessCodeAsync(Guid residentId, AccessCodeType type);
        Task<AccessCodeValidationResult> ValidateAccessCodeAsync(string code);
    }
}
