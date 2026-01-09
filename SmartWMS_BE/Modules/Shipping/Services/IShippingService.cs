using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Shipping.DTOs;
using SmartWMS.API.Modules.Shipping.Models;

namespace SmartWMS.API.Modules.Shipping.Services;

public interface IShippingService
{
    // Delivery Routes CRUD
    Task<ApiResponse<PaginatedResult<DeliveryRouteSummaryDto>>> GetRoutesAsync(
        Guid tenantId, RouteFilterRequest filter);
    Task<ApiResponse<DeliveryRouteDto>> GetRouteByIdAsync(Guid tenantId, Guid id);
    Task<ApiResponse<DeliveryRouteDto>> GetRouteByNumberAsync(Guid tenantId, string routeNumber);
    Task<ApiResponse<DeliveryRouteDto>> CreateRouteAsync(
        Guid tenantId, Guid userId, CreateDeliveryRouteRequest request);
    Task<ApiResponse<DeliveryRouteDto>> UpdateRouteAsync(
        Guid tenantId, Guid id, UpdateDeliveryRouteRequest request);
    Task<ApiResponse<bool>> DeleteRouteAsync(Guid tenantId, Guid id);

    // Delivery Stops
    Task<ApiResponse<DeliveryStopDto>> AddStopAsync(
        Guid tenantId, Guid routeId, CreateDeliveryStopRequest request);
    Task<ApiResponse<DeliveryStopDto>> UpdateStopAsync(
        Guid tenantId, Guid routeId, Guid stopId, UpdateDeliveryStopRequest request);
    Task<ApiResponse<bool>> RemoveStopAsync(Guid tenantId, Guid routeId, Guid stopId);
    Task<ApiResponse<DeliveryRouteDto>> ReorderStopsAsync(
        Guid tenantId, Guid routeId, List<Guid> stopIdsInOrder);

    // Route Workflow
    Task<ApiResponse<DeliveryRouteDto>> ReleaseRouteAsync(
        Guid tenantId, Guid id, Guid userId, ReleaseRouteRequest request);
    Task<ApiResponse<DeliveryRouteDto>> StartLoadingAsync(
        Guid tenantId, Guid id, Guid userId, StartLoadingRequest request);
    Task<ApiResponse<DeliveryRouteDto>> CompleteLoadingAsync(
        Guid tenantId, Guid id, Guid userId, CompleteLoadingRequest request);
    Task<ApiResponse<DeliveryRouteDto>> DepartAsync(
        Guid tenantId, Guid id, Guid userId, DepartRouteRequest request);
    Task<ApiResponse<DeliveryRouteDto>> ReturnToWarehouseAsync(
        Guid tenantId, Guid id, Guid userId, ReturnToWarehouseRequest request);
    Task<ApiResponse<DeliveryRouteDto>> CancelRouteAsync(Guid tenantId, Guid id, Guid userId);

    // Stop Workflow
    Task<ApiResponse<DeliveryStopDto>> ArriveAtStopAsync(
        Guid tenantId, Guid routeId, Guid stopId, Guid userId, ArriveAtStopRequest request);
    Task<ApiResponse<DeliveryStopDto>> CompleteStopDeliveryAsync(
        Guid tenantId, Guid routeId, Guid stopId, Guid userId, CompleteStopDeliveryRequest request);
    Task<ApiResponse<DeliveryStopDto>> FailStopDeliveryAsync(
        Guid tenantId, Guid routeId, Guid stopId, Guid userId, FailStopDeliveryRequest request);
    Task<ApiResponse<DeliveryStopDto>> SkipStopAsync(
        Guid tenantId, Guid routeId, Guid stopId, Guid userId, string? reason);

    // Tracking
    Task<ApiResponse<List<DeliveryTrackingEventDto>>> GetTrackingEventsAsync(
        Guid tenantId, Guid routeId);
    Task<ApiResponse<DeliveryTrackingEventDto>> AddTrackingEventAsync(
        Guid tenantId, Guid routeId, Guid userId, AddTrackingEventRequest request);

    // Auto-planning
    Task<ApiResponse<DeliveryRouteDto>> AutoPlanRouteAsync(
        Guid tenantId, Guid userId, AutoPlanRouteRequest request);
    Task<ApiResponse<DeliveryRouteDto>> OptimizeRouteAsync(
        Guid tenantId, Guid id, OptimizeRouteRequest request);

    // Shipment assignment
    Task<ApiResponse<DeliveryStopDto>> AssignShipmentToRouteAsync(
        Guid tenantId, Guid routeId, Guid shipmentId);
    Task<ApiResponse<bool>> UnassignShipmentFromRouteAsync(
        Guid tenantId, Guid routeId, Guid shipmentId);
}
