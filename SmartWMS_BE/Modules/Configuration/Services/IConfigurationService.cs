using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Configuration.DTOs;
using SmartWMS.API.Modules.Configuration.Models;

namespace SmartWMS.API.Modules.Configuration.Services;

public interface IConfigurationService
{
    // Barcode Prefixes
    Task<ApiResponse<List<BarcodePrefixDto>>> GetBarcodePrefixesAsync(Guid tenantId, BarcodePrefixType? prefixType = null);
    Task<ApiResponse<BarcodePrefixDto>> GetBarcodePrefixByIdAsync(Guid tenantId, Guid id);
    Task<ApiResponse<BarcodePrefixDto>> CreateBarcodePrefixAsync(Guid tenantId, CreateBarcodePrefixRequest request);
    Task<ApiResponse<BarcodePrefixDto>> UpdateBarcodePrefixAsync(Guid tenantId, Guid id, UpdateBarcodePrefixRequest request);
    Task<ApiResponse<bool>> DeleteBarcodePrefixAsync(Guid tenantId, Guid id);
    Task<ApiResponse<BarcodePrefixType?>> IdentifyBarcodeTypeAsync(Guid tenantId, string barcode);

    // Reason Codes
    Task<ApiResponse<List<ReasonCodeDto>>> GetReasonCodesAsync(Guid tenantId, ReasonCodeType? reasonType = null);
    Task<ApiResponse<ReasonCodeDto>> GetReasonCodeByIdAsync(Guid tenantId, Guid id);
    Task<ApiResponse<ReasonCodeDto>> CreateReasonCodeAsync(Guid tenantId, CreateReasonCodeRequest request);
    Task<ApiResponse<ReasonCodeDto>> UpdateReasonCodeAsync(Guid tenantId, Guid id, UpdateReasonCodeRequest request);
    Task<ApiResponse<bool>> DeleteReasonCodeAsync(Guid tenantId, Guid id);

    // Number Sequences
    Task<ApiResponse<List<NumberSequenceDto>>> GetNumberSequencesAsync(Guid tenantId);
    Task<ApiResponse<NumberSequenceDto>> GetNumberSequenceByTypeAsync(Guid tenantId, string sequenceType);
    Task<ApiResponse<NumberSequenceDto>> CreateNumberSequenceAsync(Guid tenantId, CreateNumberSequenceRequest request);
    Task<ApiResponse<NumberSequenceDto>> UpdateNumberSequenceAsync(Guid tenantId, Guid id, UpdateNumberSequenceRequest request);
    Task<ApiResponse<string>> GetNextNumberAsync(Guid tenantId, string sequenceType);

    // System Settings
    Task<ApiResponse<List<SystemSettingDto>>> GetSystemSettingsAsync(Guid tenantId, string? category = null);
    Task<ApiResponse<SystemSettingDto>> GetSystemSettingAsync(Guid tenantId, string category, string key);
    Task<ApiResponse<SystemSettingDto>> CreateSystemSettingAsync(Guid tenantId, CreateSystemSettingRequest request);
    Task<ApiResponse<SystemSettingDto>> UpdateSystemSettingAsync(Guid tenantId, Guid id, UpdateSystemSettingRequest request);
    Task<ApiResponse<bool>> DeleteSystemSettingAsync(Guid tenantId, Guid id);
}
