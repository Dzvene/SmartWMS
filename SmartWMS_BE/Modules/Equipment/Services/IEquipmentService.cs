using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Equipment.DTOs;

namespace SmartWMS.API.Modules.Equipment.Services;

public interface IEquipmentService
{
    Task<ApiResponse<PaginatedResult<EquipmentDto>>> GetEquipmentAsync(
        Guid tenantId,
        EquipmentFilters? filters = null,
        int page = 1,
        int pageSize = 25);

    Task<ApiResponse<EquipmentDto>> GetEquipmentByIdAsync(Guid tenantId, Guid id);

    Task<ApiResponse<EquipmentDto>> GetEquipmentByCodeAsync(Guid tenantId, string code);

    Task<ApiResponse<EquipmentDto>> CreateEquipmentAsync(Guid tenantId, CreateEquipmentRequest request);

    Task<ApiResponse<EquipmentDto>> UpdateEquipmentAsync(Guid tenantId, Guid id, UpdateEquipmentRequest request);

    Task<ApiResponse> DeleteEquipmentAsync(Guid tenantId, Guid id);

    Task<ApiResponse<EquipmentDto>> AssignEquipmentAsync(Guid tenantId, Guid id, AssignEquipmentRequest request);

    Task<ApiResponse<EquipmentDto>> UpdateEquipmentStatusAsync(Guid tenantId, Guid id, UpdateEquipmentStatusRequest request);

    Task<ApiResponse<IEnumerable<EquipmentDto>>> GetAvailableEquipmentAsync(
        Guid tenantId,
        Models.EquipmentType? type = null,
        Guid? warehouseId = null);
}
