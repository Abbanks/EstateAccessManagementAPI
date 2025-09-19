using EstateAccessManagement.Application.Features.AccessCodes.DTOs;
using EstateAccessManagement.Common.Enums;

namespace EstateAccessManagement.Application.Interfaces
{
    public interface IAccessCodeService
    {
        Task<GenerateAccessCodeResult> GenerateAccessCodeAsync(Guid residentId, AccessCodeType type);
        Task<AccessCodeValidationResult> ValidateAccessCodeAsync(string code);
        Task<GetAccessCodeResult> GetAccessCodeByIdAsync(Guid Id);
    }
}
