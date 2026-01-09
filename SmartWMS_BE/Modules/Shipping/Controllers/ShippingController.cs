using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWMS.API.Modules.Shipping.DTOs;
using SmartWMS.API.Modules.Shipping.Models;
using SmartWMS.API.Modules.Shipping.Services;
using System.Security.Claims;

namespace SmartWMS.API.Modules.Shipping.Controllers;

[ApiController]
[Route("api/v1/tenant/{tenantId}/delivery-routes")]
[Authorize]
public class ShippingController : ControllerBase
{
    private readonly IShippingService _shippingService;

    public ShippingController(IShippingService shippingService)
    {
        _shippingService = shippingService;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    #region Delivery Routes CRUD

    [HttpGet]
    public async Task<IActionResult> GetRoutes(
        Guid tenantId,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] Guid? carrierId = null,
        [FromQuery] RouteStatus? status = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] string? driverName = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var filter = new RouteFilterRequest(
            warehouseId, carrierId, status, dateFrom, dateTo,
            driverName, searchTerm, page, pageSize);

        var result = await _shippingService.GetRoutesAsync(tenantId, filter);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRouteById(Guid tenantId, Guid id)
    {
        var result = await _shippingService.GetRouteByIdAsync(tenantId, id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet("by-number/{routeNumber}")]
    public async Task<IActionResult> GetRouteByNumber(Guid tenantId, string routeNumber)
    {
        var result = await _shippingService.GetRouteByNumberAsync(tenantId, routeNumber);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRoute(
        Guid tenantId, [FromBody] CreateDeliveryRouteRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _shippingService.CreateRouteAsync(tenantId, userId, request);
        return result.Success
            ? CreatedAtAction(nameof(GetRouteById), new { tenantId, id = result.Data!.Id }, result)
            : BadRequest(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRoute(
        Guid tenantId, Guid id, [FromBody] UpdateDeliveryRouteRequest request)
    {
        var result = await _shippingService.UpdateRouteAsync(tenantId, id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRoute(Guid tenantId, Guid id)
    {
        var result = await _shippingService.DeleteRouteAsync(tenantId, id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Delivery Stops

    [HttpPost("{routeId}/stops")]
    public async Task<IActionResult> AddStop(
        Guid tenantId, Guid routeId, [FromBody] CreateDeliveryStopRequest request)
    {
        var result = await _shippingService.AddStopAsync(tenantId, routeId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{routeId}/stops/{stopId}")]
    public async Task<IActionResult> UpdateStop(
        Guid tenantId, Guid routeId, Guid stopId, [FromBody] UpdateDeliveryStopRequest request)
    {
        var result = await _shippingService.UpdateStopAsync(tenantId, routeId, stopId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{routeId}/stops/{stopId}")]
    public async Task<IActionResult> RemoveStop(Guid tenantId, Guid routeId, Guid stopId)
    {
        var result = await _shippingService.RemoveStopAsync(tenantId, routeId, stopId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{routeId}/stops/reorder")]
    public async Task<IActionResult> ReorderStops(
        Guid tenantId, Guid routeId, [FromBody] List<Guid> stopIdsInOrder)
    {
        var result = await _shippingService.ReorderStopsAsync(tenantId, routeId, stopIdsInOrder);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Route Workflow

    [HttpPost("{id}/release")]
    public async Task<IActionResult> ReleaseRoute(
        Guid tenantId, Guid id, [FromBody] ReleaseRouteRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _shippingService.ReleaseRouteAsync(tenantId, id, userId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/start-loading")]
    public async Task<IActionResult> StartLoading(
        Guid tenantId, Guid id, [FromBody] StartLoadingRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _shippingService.StartLoadingAsync(tenantId, id, userId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/complete-loading")]
    public async Task<IActionResult> CompleteLoading(
        Guid tenantId, Guid id, [FromBody] CompleteLoadingRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _shippingService.CompleteLoadingAsync(tenantId, id, userId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/depart")]
    public async Task<IActionResult> Depart(
        Guid tenantId, Guid id, [FromBody] DepartRouteRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _shippingService.DepartAsync(tenantId, id, userId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/return")]
    public async Task<IActionResult> ReturnToWarehouse(
        Guid tenantId, Guid id, [FromBody] ReturnToWarehouseRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _shippingService.ReturnToWarehouseAsync(tenantId, id, userId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelRoute(Guid tenantId, Guid id)
    {
        var userId = GetCurrentUserId();
        var result = await _shippingService.CancelRouteAsync(tenantId, id, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Stop Workflow

    [HttpPost("{routeId}/stops/{stopId}/arrive")]
    public async Task<IActionResult> ArriveAtStop(
        Guid tenantId, Guid routeId, Guid stopId, [FromBody] ArriveAtStopRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _shippingService.ArriveAtStopAsync(tenantId, routeId, stopId, userId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{routeId}/stops/{stopId}/complete")]
    public async Task<IActionResult> CompleteStopDelivery(
        Guid tenantId, Guid routeId, Guid stopId, [FromBody] CompleteStopDeliveryRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _shippingService.CompleteStopDeliveryAsync(tenantId, routeId, stopId, userId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{routeId}/stops/{stopId}/fail")]
    public async Task<IActionResult> FailStopDelivery(
        Guid tenantId, Guid routeId, Guid stopId, [FromBody] FailStopDeliveryRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _shippingService.FailStopDeliveryAsync(tenantId, routeId, stopId, userId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{routeId}/stops/{stopId}/skip")]
    public async Task<IActionResult> SkipStop(
        Guid tenantId, Guid routeId, Guid stopId, [FromBody] string? reason = null)
    {
        var userId = GetCurrentUserId();
        var result = await _shippingService.SkipStopAsync(tenantId, routeId, stopId, userId, reason);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Tracking

    [HttpGet("{routeId}/tracking")]
    public async Task<IActionResult> GetTrackingEvents(Guid tenantId, Guid routeId)
    {
        var result = await _shippingService.GetTrackingEventsAsync(tenantId, routeId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("{routeId}/tracking")]
    public async Task<IActionResult> AddTrackingEvent(
        Guid tenantId, Guid routeId, [FromBody] AddTrackingEventRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _shippingService.AddTrackingEventAsync(tenantId, routeId, userId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Auto-planning

    [HttpPost("auto-plan")]
    public async Task<IActionResult> AutoPlanRoute(
        Guid tenantId, [FromBody] AutoPlanRouteRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _shippingService.AutoPlanRouteAsync(tenantId, userId, request);
        return result.Success
            ? CreatedAtAction(nameof(GetRouteById), new { tenantId, id = result.Data!.Id }, result)
            : BadRequest(result);
    }

    [HttpPost("{id}/optimize")]
    public async Task<IActionResult> OptimizeRoute(
        Guid tenantId, Guid id, [FromBody] OptimizeRouteRequest request)
    {
        var result = await _shippingService.OptimizeRouteAsync(tenantId, id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion

    #region Shipment Assignment

    [HttpPost("{routeId}/shipments/{shipmentId}")]
    public async Task<IActionResult> AssignShipment(Guid tenantId, Guid routeId, Guid shipmentId)
    {
        var result = await _shippingService.AssignShipmentToRouteAsync(tenantId, routeId, shipmentId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{routeId}/shipments/{shipmentId}")]
    public async Task<IActionResult> UnassignShipment(Guid tenantId, Guid routeId, Guid shipmentId)
    {
        var result = await _shippingService.UnassignShipmentFromRouteAsync(tenantId, routeId, shipmentId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    #endregion
}
