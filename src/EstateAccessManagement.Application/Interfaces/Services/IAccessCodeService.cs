using EstateAccessManagement.Application.Features.AccessCodes.DTOs;
using EstateAccessManagement.Core.Enums;

namespace EstateAccessManagement.Application.Interfaces.Services
{
    public interface IAccessCodeService
    {
        Task<GenerateAccessCodeResult> GenerateAccessCodeAsync(Guid residentId, AccessCodeType type);
        Task<AccessCodeValidationResult> ValidateAccessCodeAsync(string code);
        Task<GetAccessCodeResult> GetAccessCodeByIdAsync(Guid Id);
        Task<List<GetAccessCodeResult>?> GetAccessCodes(Guid id);
        Task<bool> DeleteAccessCodeAsync(Guid id);
    }
}
