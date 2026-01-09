using Microsoft.EntityFrameworkCore;
using SmartWMS.API.Common.Models;
using SmartWMS.API.Infrastructure.Data;
using SmartWMS.API.Modules.Shipping.DTOs;
using SmartWMS.API.Modules.Shipping.Models;
using SmartWMS.API.Modules.Automation.Services;

namespace SmartWMS.API.Modules.Shipping.Services;

public class ShippingService : IShippingService
{
    private readonly ApplicationDbContext _context;
    private readonly IAutomationEventPublisher _automationEvents;

    public ShippingService(ApplicationDbContext context, IAutomationEventPublisher automationEvents)
    {
        _context = context;
        _automationEvents = automationEvents;
    }

    #region Delivery Routes CRUD

    public async Task<ApiResponse<PaginatedResult<DeliveryRouteSummaryDto>>> GetRoutesAsync(
        Guid tenantId, RouteFilterRequest filter)
    {
        var query = _context.DeliveryRoutes
            .Include(r => r.Warehouse)
            .Include(r => r.Carrier)
            .Include(r => r.Stops)
            .Where(r => r.TenantId == tenantId);

        // Apply filters
        if (filter.WarehouseId.HasValue)
            query = query.Where(r => r.WarehouseId == filter.WarehouseId.Value);

        if (filter.CarrierId.HasValue)
            query = query.Where(r => r.CarrierId == filter.CarrierId.Value);

        if (filter.Status.HasValue)
            query = query.Where(r => r.Status == filter.Status.Value);

        if (filter.DateFrom.HasValue)
            query = query.Where(r => r.PlannedDate >= filter.DateFrom.Value);

        if (filter.DateTo.HasValue)
            query = query.Where(r => r.PlannedDate <= filter.DateTo.Value);

        if (!string.IsNullOrWhiteSpace(filter.DriverName))
            query = query.Where(r => r.DriverName != null && r.DriverName.ToLower().Contains(filter.DriverName.ToLower()));

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.ToLower();
            query = query.Where(r =>
                r.RouteNumber.ToLower().Contains(term) ||
                (r.RouteName != null && r.RouteName.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(r => r.PlannedDate)
            .ThenByDescending(r => r.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(r => new DeliveryRouteSummaryDto(
                r.Id,
                r.RouteNumber,
                r.RouteName,
                r.Warehouse.Name,
                r.Carrier != null ? r.Carrier.Name : null,
                r.DriverName,
                r.PlannedDate,
                r.Status,
                r.Stops.Count,
                r.Stops.Count(s => s.Status == StopStatus.Complete),
                r.CreatedAt
            ))
            .ToListAsync();

        var result = new PaginatedResult<DeliveryRouteSummaryDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };

        return ApiResponse<PaginatedResult<DeliveryRouteSummaryDto>>.Ok(result);
    }

    public async Task<ApiResponse<DeliveryRouteDto>> GetRouteByIdAsync(Guid tenantId, Guid id)
    {
        var route = await _context.DeliveryRoutes
            .Include(r => r.Warehouse)
            .Include(r => r.Carrier)
            .Include(r => r.CarrierService)
            .Include(r => r.Stops.OrderBy(s => s.StopSequence))
                .ThenInclude(s => s.Shipment)
            .Include(r => r.Stops)
                .ThenInclude(s => s.Customer)
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == id);

        if (route == null)
            return ApiResponse<DeliveryRouteDto>.Fail("Route not found");

        return ApiResponse<DeliveryRouteDto>.Ok(MapToDto(route));
    }

    public async Task<ApiResponse<DeliveryRouteDto>> GetRouteByNumberAsync(Guid tenantId, string routeNumber)
    {
        var route = await _context.DeliveryRoutes
            .Include(r => r.Warehouse)
            .Include(r => r.Carrier)
            .Include(r => r.Stops.OrderBy(s => s.StopSequence))
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.RouteNumber == routeNumber);

        if (route == null)
            return ApiResponse<DeliveryRouteDto>.Fail("Route not found");

        return ApiResponse<DeliveryRouteDto>.Ok(MapToDto(route));
    }

    public async Task<ApiResponse<DeliveryRouteDto>> CreateRouteAsync(
        Guid tenantId, Guid userId, CreateDeliveryRouteRequest request)
    {
        // Validate warehouse
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.TenantId == tenantId && w.Id == request.WarehouseId);

        if (warehouse == null)
            return ApiResponse<DeliveryRouteDto>.Fail("Warehouse not found");

        var routeNumber = await GenerateRouteNumberAsync(tenantId);

        var route = new DeliveryRoute
        {
            TenantId = tenantId,
            RouteNumber = routeNumber,
            RouteName = request.RouteName,
            WarehouseId = request.WarehouseId,
            CarrierId = request.CarrierId,
            CarrierServiceId = request.CarrierServiceId,
            DriverName = request.DriverName,
            VehicleNumber = request.VehicleNumber,
            VehicleType = request.VehicleType,
            PlannedDate = request.PlannedDate,
            PlannedDepartureTime = request.PlannedDepartureTime,
            PlannedReturnTime = request.PlannedReturnTime,
            MaxWeight = request.MaxWeight,
            MaxVolume = request.MaxVolume,
            MaxStops = request.MaxStops,
            Notes = request.Notes,
            Status = RouteStatus.Draft
        };

        _context.DeliveryRoutes.Add(route);

        // Add stops if provided
        if (request.Stops?.Any() == true)
        {
            int sequence = 1;
            foreach (var stopRequest in request.Stops)
            {
                var stop = CreateStopFromRequest(tenantId, route.Id, sequence++, stopRequest);
                route.Stops.Add(stop);
            }
            route.EstimatedStops = route.Stops.Count;
        }

        await _context.SaveChangesAsync();

        // Publish automation event
        await _automationEvents.PublishEntityCreatedAsync(tenantId, route);

        return await GetRouteByIdAsync(tenantId, route.Id);
    }

    public async Task<ApiResponse<DeliveryRouteDto>> UpdateRouteAsync(
        Guid tenantId, Guid id, UpdateDeliveryRouteRequest request)
    {
        var route = await _context.DeliveryRoutes
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == id);

        if (route == null)
            return ApiResponse<DeliveryRouteDto>.Fail("Route not found");

        if (route.Status != RouteStatus.Draft && route.Status != RouteStatus.Planned)
            return ApiResponse<DeliveryRouteDto>.Fail("Can only update Draft or Planned routes");

        if (request.RouteName != null) route.RouteName = request.RouteName;
        if (request.CarrierId.HasValue) route.CarrierId = request.CarrierId;
        if (request.CarrierServiceId.HasValue) route.CarrierServiceId = request.CarrierServiceId;
        if (request.DriverName != null) route.DriverName = request.DriverName;
        if (request.VehicleNumber != null) route.VehicleNumber = request.VehicleNumber;
        if (request.VehicleType != null) route.VehicleType = request.VehicleType;
        if (request.PlannedDate.HasValue) route.PlannedDate = request.PlannedDate.Value;
        if (request.PlannedDepartureTime.HasValue) route.PlannedDepartureTime = request.PlannedDepartureTime;
        if (request.PlannedReturnTime.HasValue) route.PlannedReturnTime = request.PlannedReturnTime;
        if (request.MaxWeight.HasValue) route.MaxWeight = request.MaxWeight;
        if (request.MaxVolume.HasValue) route.MaxVolume = request.MaxVolume;
        if (request.MaxStops.HasValue) route.MaxStops = request.MaxStops;
        if (request.Notes != null) route.Notes = request.Notes;

        await _context.SaveChangesAsync();

        return await GetRouteByIdAsync(tenantId, id);
    }

    public async Task<ApiResponse<bool>> DeleteRouteAsync(Guid tenantId, Guid id)
    {
        var route = await _context.DeliveryRoutes
            .Include(r => r.Stops)
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == id);

        if (route == null)
            return ApiResponse<bool>.Fail("Route not found");

        if (route.Status != RouteStatus.Draft)
            return ApiResponse<bool>.Fail("Can only delete Draft routes");

        _context.DeliveryStops.RemoveRange(route.Stops);
        _context.DeliveryRoutes.Remove(route);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true);
    }

    #endregion

    #region Delivery Stops

    public async Task<ApiResponse<DeliveryStopDto>> AddStopAsync(
        Guid tenantId, Guid routeId, CreateDeliveryStopRequest request)
    {
        var route = await _context.DeliveryRoutes
            .Include(r => r.Stops)
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == routeId);

        if (route == null)
            return ApiResponse<DeliveryStopDto>.Fail("Route not found");

        if (route.Status != RouteStatus.Draft && route.Status != RouteStatus.Planned)
            return ApiResponse<DeliveryStopDto>.Fail("Cannot add stops to routes in progress");

        var sequence = route.Stops.Count > 0 ? route.Stops.Max(s => s.StopSequence) + 1 : 1;
        var stop = CreateStopFromRequest(tenantId, routeId, sequence, request);

        _context.DeliveryStops.Add(stop);
        route.EstimatedStops = route.Stops.Count + 1;

        await _context.SaveChangesAsync();

        return ApiResponse<DeliveryStopDto>.Ok(MapStopToDto(stop));
    }

    public async Task<ApiResponse<DeliveryStopDto>> UpdateStopAsync(
        Guid tenantId, Guid routeId, Guid stopId, UpdateDeliveryStopRequest request)
    {
        var route = await _context.DeliveryRoutes
            .Include(r => r.Stops)
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == routeId);

        if (route == null)
            return ApiResponse<DeliveryStopDto>.Fail("Route not found");

        var stop = route.Stops.FirstOrDefault(s => s.Id == stopId);
        if (stop == null)
            return ApiResponse<DeliveryStopDto>.Fail("Stop not found");

        if (request.StopSequence.HasValue) stop.StopSequence = request.StopSequence.Value;
        if (request.CustomerName != null) stop.CustomerName = request.CustomerName;
        if (request.AddressLine1 != null) stop.AddressLine1 = request.AddressLine1;
        if (request.City != null) stop.City = request.City;
        if (request.ContactPhone != null) stop.ContactPhone = request.ContactPhone;
        if (request.DeliveryInstructions != null) stop.DeliveryInstructions = request.DeliveryInstructions;
        if (request.TimeWindowStart.HasValue) stop.TimeWindowStart = request.TimeWindowStart;
        if (request.TimeWindowEnd.HasValue) stop.TimeWindowEnd = request.TimeWindowEnd;
        if (request.Notes != null) stop.Notes = request.Notes;

        await _context.SaveChangesAsync();

        return ApiResponse<DeliveryStopDto>.Ok(MapStopToDto(stop));
    }

    public async Task<ApiResponse<bool>> RemoveStopAsync(Guid tenantId, Guid routeId, Guid stopId)
    {
        var route = await _context.DeliveryRoutes
            .Include(r => r.Stops)
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == routeId);

        if (route == null)
            return ApiResponse<bool>.Fail("Route not found");

        if (route.Status != RouteStatus.Draft)
            return ApiResponse<bool>.Fail("Can only remove stops from Draft routes");

        var stop = route.Stops.FirstOrDefault(s => s.Id == stopId);
        if (stop == null)
            return ApiResponse<bool>.Fail("Stop not found");

        route.Stops.Remove(stop);
        _context.DeliveryStops.Remove(stop);

        // Resequence remaining stops
        var sequence = 1;
        foreach (var s in route.Stops.OrderBy(s => s.StopSequence))
        {
            s.StopSequence = sequence++;
        }

        route.EstimatedStops = route.Stops.Count;

        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true);
    }

    public async Task<ApiResponse<DeliveryRouteDto>> ReorderStopsAsync(
        Guid tenantId, Guid routeId, List<Guid> stopIdsInOrder)
    {
        var route = await _context.DeliveryRoutes
            .Include(r => r.Stops)
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == routeId);

        if (route == null)
            return ApiResponse<DeliveryRouteDto>.Fail("Route not found");

        if (route.Status != RouteStatus.Draft && route.Status != RouteStatus.Planned)
            return ApiResponse<DeliveryRouteDto>.Fail("Can only reorder stops in Draft or Planned routes");

        var sequence = 1;
        foreach (var stopId in stopIdsInOrder)
        {
            var stop = route.Stops.FirstOrDefault(s => s.Id == stopId);
            if (stop != null)
            {
                stop.StopSequence = sequence++;
            }
        }

        await _context.SaveChangesAsync();

        return await GetRouteByIdAsync(tenantId, routeId);
    }

    #endregion

    #region Route Workflow

    public async Task<ApiResponse<DeliveryRouteDto>> ReleaseRouteAsync(
        Guid tenantId, Guid id, Guid userId, ReleaseRouteRequest request)
    {
        var route = await _context.DeliveryRoutes
            .Include(r => r.Stops)
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == id);

        if (route == null)
            return ApiResponse<DeliveryRouteDto>.Fail("Route not found");

        if (route.Status != RouteStatus.Draft && route.Status != RouteStatus.Planned)
            return ApiResponse<DeliveryRouteDto>.Fail("Can only release Draft or Planned routes");

        if (!route.Stops.Any())
            return ApiResponse<DeliveryRouteDto>.Fail("Cannot release route without stops");

        var previousStatus = route.Status.ToString();
        route.Status = RouteStatus.Released;

        // Add tracking event
        await AddTrackingEventInternal(tenantId, route.Id, null, null,
            TrackingEventType.Released, "Route released for delivery", null, null, null);

        await _context.SaveChangesAsync();

        // Publish automation event for status change
        await _automationEvents.PublishStatusChangedAsync(tenantId, route, previousStatus, route.Status.ToString());

        return await GetRouteByIdAsync(tenantId, id);
    }

    public async Task<ApiResponse<DeliveryRouteDto>> StartLoadingAsync(
        Guid tenantId, Guid id, Guid userId, StartLoadingRequest request)
    {
        var route = await _context.DeliveryRoutes
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == id);

        if (route == null)
            return ApiResponse<DeliveryRouteDto>.Fail("Route not found");

        if (route.Status != RouteStatus.Released)
            return ApiResponse<DeliveryRouteDto>.Fail("Route must be Released to start loading");

        route.Status = RouteStatus.Loading;

        await AddTrackingEventInternal(tenantId, route.Id, null, null,
            TrackingEventType.LoadingStarted, "Vehicle loading started", null, null, null);

        await _context.SaveChangesAsync();

        return await GetRouteByIdAsync(tenantId, id);
    }

    public async Task<ApiResponse<DeliveryRouteDto>> CompleteLoadingAsync(
        Guid tenantId, Guid id, Guid userId, CompleteLoadingRequest request)
    {
        var route = await _context.DeliveryRoutes
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == id);

        if (route == null)
            return ApiResponse<DeliveryRouteDto>.Fail("Route not found");

        if (route.Status != RouteStatus.Loading)
            return ApiResponse<DeliveryRouteDto>.Fail("Route must be Loading to complete loading");

        if (request.ActualWeight.HasValue)
            route.CurrentWeight = request.ActualWeight.Value;
        if (request.ActualVolume.HasValue)
            route.CurrentVolume = request.ActualVolume.Value;

        await AddTrackingEventInternal(tenantId, route.Id, null, null,
            TrackingEventType.LoadingComplete, "Vehicle loading completed", null, null, null);

        await _context.SaveChangesAsync();

        return await GetRouteByIdAsync(tenantId, id);
    }

    public async Task<ApiResponse<DeliveryRouteDto>> DepartAsync(
        Guid tenantId, Guid id, Guid userId, DepartRouteRequest request)
    {
        var route = await _context.DeliveryRoutes
            .Include(r => r.Stops)
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == id);

        if (route == null)
            return ApiResponse<DeliveryRouteDto>.Fail("Route not found");

        if (route.Status != RouteStatus.Loading && route.Status != RouteStatus.Released)
            return ApiResponse<DeliveryRouteDto>.Fail("Route must be Released or Loading to depart");

        route.Status = RouteStatus.InTransit;
        route.ActualDepartureTime = request.DepartureTime ?? DateTime.UtcNow;

        // Mark first stop as en route
        var firstStop = route.Stops.OrderBy(s => s.StopSequence).FirstOrDefault();
        if (firstStop != null)
        {
            firstStop.Status = StopStatus.EnRoute;
        }

        await AddTrackingEventInternal(tenantId, route.Id, null, null,
            TrackingEventType.Departed, "Departed warehouse", null, null, null);

        await _context.SaveChangesAsync();

        return await GetRouteByIdAsync(tenantId, id);
    }

    public async Task<ApiResponse<DeliveryRouteDto>> ReturnToWarehouseAsync(
        Guid tenantId, Guid id, Guid userId, ReturnToWarehouseRequest request)
    {
        var route = await _context.DeliveryRoutes
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == id);

        if (route == null)
            return ApiResponse<DeliveryRouteDto>.Fail("Route not found");

        route.Status = RouteStatus.Complete;
        route.ActualReturnTime = request.ReturnTime ?? DateTime.UtcNow;

        await AddTrackingEventInternal(tenantId, route.Id, null, null,
            TrackingEventType.ReturnedToWarehouse, "Returned to warehouse", null, null, null);

        await _context.SaveChangesAsync();

        return await GetRouteByIdAsync(tenantId, id);
    }

    public async Task<ApiResponse<DeliveryRouteDto>> CancelRouteAsync(Guid tenantId, Guid id, Guid userId)
    {
        var route = await _context.DeliveryRoutes
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == id);

        if (route == null)
            return ApiResponse<DeliveryRouteDto>.Fail("Route not found");

        if (route.Status == RouteStatus.Complete)
            return ApiResponse<DeliveryRouteDto>.Fail("Cannot cancel completed routes");

        route.Status = RouteStatus.Cancelled;

        await AddTrackingEventInternal(tenantId, route.Id, null, null,
            TrackingEventType.Cancelled, "Route cancelled", null, null, null);

        await _context.SaveChangesAsync();

        return await GetRouteByIdAsync(tenantId, id);
    }

    #endregion

    #region Stop Workflow

    public async Task<ApiResponse<DeliveryStopDto>> ArriveAtStopAsync(
        Guid tenantId, Guid routeId, Guid stopId, Guid userId, ArriveAtStopRequest request)
    {
        var route = await _context.DeliveryRoutes
            .Include(r => r.Stops)
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == routeId);

        if (route == null)
            return ApiResponse<DeliveryStopDto>.Fail("Route not found");

        var stop = route.Stops.FirstOrDefault(s => s.Id == stopId);
        if (stop == null)
            return ApiResponse<DeliveryStopDto>.Fail("Stop not found");

        stop.Status = StopStatus.Arrived;
        stop.ArrivalTime = request.ArrivalTime ?? DateTime.UtcNow;

        route.Status = RouteStatus.Delivering;

        await AddTrackingEventInternal(tenantId, routeId, stopId, stop.ShipmentId,
            TrackingEventType.ArrivedAtStop, $"Arrived at stop {stop.StopSequence}",
            request.Latitude, request.Longitude, stop.AddressLine1);

        await _context.SaveChangesAsync();

        return ApiResponse<DeliveryStopDto>.Ok(MapStopToDto(stop));
    }

    public async Task<ApiResponse<DeliveryStopDto>> CompleteStopDeliveryAsync(
        Guid tenantId, Guid routeId, Guid stopId, Guid userId, CompleteStopDeliveryRequest request)
    {
        var route = await _context.DeliveryRoutes
            .Include(r => r.Stops)
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == routeId);

        if (route == null)
            return ApiResponse<DeliveryStopDto>.Fail("Route not found");

        var stop = route.Stops.FirstOrDefault(s => s.Id == stopId);
        if (stop == null)
            return ApiResponse<DeliveryStopDto>.Fail("Stop not found");

        stop.Status = request.IsPartial ? StopStatus.PartialDelivery : StopStatus.Complete;
        stop.DepartureTime = DateTime.UtcNow;
        stop.SignedBy = request.SignedBy;
        stop.SignedAt = DateTime.UtcNow;
        stop.ProofOfDeliveryUrl = request.ProofOfDeliveryUrl;
        stop.IssueType = request.IssueType;
        stop.IssueNotes = request.IssueNotes;

        // Mark next stop as en route
        var nextStop = route.Stops
            .Where(s => s.StopSequence > stop.StopSequence && s.Status == StopStatus.Pending)
            .OrderBy(s => s.StopSequence)
            .FirstOrDefault();

        if (nextStop != null)
        {
            nextStop.Status = StopStatus.EnRoute;
        }
        else
        {
            // No more stops, returning
            route.Status = RouteStatus.Returning;
        }

        await AddTrackingEventInternal(tenantId, routeId, stopId, stop.ShipmentId,
            request.IsPartial ? TrackingEventType.PartialDelivery : TrackingEventType.DeliveryComplete,
            $"Delivery completed at stop {stop.StopSequence}", null, null, null);

        await _context.SaveChangesAsync();

        return ApiResponse<DeliveryStopDto>.Ok(MapStopToDto(stop));
    }

    public async Task<ApiResponse<DeliveryStopDto>> FailStopDeliveryAsync(
        Guid tenantId, Guid routeId, Guid stopId, Guid userId, FailStopDeliveryRequest request)
    {
        var route = await _context.DeliveryRoutes
            .Include(r => r.Stops)
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == routeId);

        if (route == null)
            return ApiResponse<DeliveryStopDto>.Fail("Route not found");

        var stop = route.Stops.FirstOrDefault(s => s.Id == stopId);
        if (stop == null)
            return ApiResponse<DeliveryStopDto>.Fail("Stop not found");

        stop.Status = StopStatus.Failed;
        stop.DepartureTime = DateTime.UtcNow;
        stop.IssueType = request.IssueType;
        stop.IssueNotes = request.IssueNotes;

        await AddTrackingEventInternal(tenantId, routeId, stopId, stop.ShipmentId,
            TrackingEventType.DeliveryFailed, $"Delivery failed: {request.IssueType}", null, null, null);

        await _context.SaveChangesAsync();

        return ApiResponse<DeliveryStopDto>.Ok(MapStopToDto(stop));
    }

    public async Task<ApiResponse<DeliveryStopDto>> SkipStopAsync(
        Guid tenantId, Guid routeId, Guid stopId, Guid userId, string? reason)
    {
        var route = await _context.DeliveryRoutes
            .Include(r => r.Stops)
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == routeId);

        if (route == null)
            return ApiResponse<DeliveryStopDto>.Fail("Route not found");

        var stop = route.Stops.FirstOrDefault(s => s.Id == stopId);
        if (stop == null)
            return ApiResponse<DeliveryStopDto>.Fail("Stop not found");

        stop.Status = StopStatus.Skipped;
        stop.IssueNotes = reason;

        await _context.SaveChangesAsync();

        return ApiResponse<DeliveryStopDto>.Ok(MapStopToDto(stop));
    }

    #endregion

    #region Tracking

    public async Task<ApiResponse<List<DeliveryTrackingEventDto>>> GetTrackingEventsAsync(
        Guid tenantId, Guid routeId)
    {
        var events = await _context.DeliveryTrackingEvents
            .Where(e => e.TenantId == tenantId && e.RouteId == routeId)
            .OrderByDescending(e => e.EventTime)
            .Select(e => new DeliveryTrackingEventDto(
                e.Id,
                e.RouteId,
                e.StopId,
                e.ShipmentId,
                e.EventType,
                e.EventTime,
                e.EventDescription,
                e.Latitude,
                e.Longitude,
                e.LocationDescription,
                e.PerformedBy,
                e.Notes
            ))
            .ToListAsync();

        return ApiResponse<List<DeliveryTrackingEventDto>>.Ok(events);
    }

    public async Task<ApiResponse<DeliveryTrackingEventDto>> AddTrackingEventAsync(
        Guid tenantId, Guid routeId, Guid userId, AddTrackingEventRequest request)
    {
        var trackingEvent = await AddTrackingEventInternal(
            tenantId, routeId, request.StopId, request.ShipmentId,
            request.EventType, request.EventDescription,
            request.Latitude, request.Longitude, request.LocationDescription);

        await _context.SaveChangesAsync();

        return ApiResponse<DeliveryTrackingEventDto>.Ok(new DeliveryTrackingEventDto(
            trackingEvent.Id,
            trackingEvent.RouteId,
            trackingEvent.StopId,
            trackingEvent.ShipmentId,
            trackingEvent.EventType,
            trackingEvent.EventTime,
            trackingEvent.EventDescription,
            trackingEvent.Latitude,
            trackingEvent.Longitude,
            trackingEvent.LocationDescription,
            trackingEvent.PerformedBy,
            trackingEvent.Notes
        ));
    }

    private async Task<DeliveryTrackingEvent> AddTrackingEventInternal(
        Guid tenantId, Guid routeId, Guid? stopId, Guid? shipmentId,
        TrackingEventType eventType, string? description,
        decimal? latitude, decimal? longitude, string? locationDescription)
    {
        var trackingEvent = new DeliveryTrackingEvent
        {
            TenantId = tenantId,
            RouteId = routeId,
            StopId = stopId,
            ShipmentId = shipmentId,
            EventType = eventType,
            EventTime = DateTime.UtcNow,
            EventDescription = description,
            Latitude = latitude,
            Longitude = longitude,
            LocationDescription = locationDescription
        };

        _context.DeliveryTrackingEvents.Add(trackingEvent);
        return trackingEvent;
    }

    #endregion

    #region Auto-planning

    public async Task<ApiResponse<DeliveryRouteDto>> AutoPlanRouteAsync(
        Guid tenantId, Guid userId, AutoPlanRouteRequest request)
    {
        // Get shipments
        var shipments = await _context.Shipments
            .Where(s => s.TenantId == tenantId && request.ShipmentIds.Contains(s.Id))
            .ToListAsync();

        if (!shipments.Any())
            return ApiResponse<DeliveryRouteDto>.Fail("No valid shipments found");

        // Create route
        var createRequest = new CreateDeliveryRouteRequest(
            $"Auto-Route {request.PlannedDate:yyyy-MM-dd}",
            request.WarehouseId,
            request.CarrierId,
            null,
            null,
            null,
            null,
            request.PlannedDate,
            null,
            null,
            request.MaxWeight,
            request.MaxVolume,
            request.MaxStops,
            "Auto-generated route",
            shipments.Select(s => new CreateDeliveryStopRequest(
                s.ShipToName,
                s.ShipToAddressLine1,
                s.ShipToAddressLine2,
                s.ShipToCity,
                s.ShipToRegion,
                s.ShipToPostalCode,
                s.ShipToCountryCode,
                null,
                null,
                null,
                null,
                null,
                null,
                s.Id,
                null,
                null
            )).ToList()
        );

        return await CreateRouteAsync(tenantId, userId, createRequest);
    }

    public async Task<ApiResponse<DeliveryRouteDto>> OptimizeRouteAsync(
        Guid tenantId, Guid id, OptimizeRouteRequest request)
    {
        // Simple implementation - in real world, would use proper routing optimization
        // For now, just return the route as-is
        return await GetRouteByIdAsync(tenantId, id);
    }

    #endregion

    #region Shipment Assignment

    public async Task<ApiResponse<DeliveryStopDto>> AssignShipmentToRouteAsync(
        Guid tenantId, Guid routeId, Guid shipmentId)
    {
        var route = await _context.DeliveryRoutes
            .Include(r => r.Stops)
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == routeId);

        if (route == null)
            return ApiResponse<DeliveryStopDto>.Fail("Route not found");

        var shipment = await _context.Shipments
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == shipmentId);

        if (shipment == null)
            return ApiResponse<DeliveryStopDto>.Fail("Shipment not found");

        // Check if shipment already assigned
        var existingStop = route.Stops.FirstOrDefault(s => s.ShipmentId == shipmentId);
        if (existingStop != null)
            return ApiResponse<DeliveryStopDto>.Fail("Shipment already assigned to this route");

        var sequence = route.Stops.Count > 0 ? route.Stops.Max(s => s.StopSequence) + 1 : 1;

        var stop = new DeliveryStop
        {
            TenantId = tenantId,
            RouteId = routeId,
            StopSequence = sequence,
            CustomerName = shipment.ShipToName,
            AddressLine1 = shipment.ShipToAddressLine1,
            AddressLine2 = shipment.ShipToAddressLine2,
            City = shipment.ShipToCity,
            State = shipment.ShipToRegion,
            PostalCode = shipment.ShipToPostalCode,
            Country = shipment.ShipToCountryCode,
            ShipmentId = shipmentId
        };

        _context.DeliveryStops.Add(stop);
        route.EstimatedStops = route.Stops.Count + 1;

        await _context.SaveChangesAsync();

        return ApiResponse<DeliveryStopDto>.Ok(MapStopToDto(stop));
    }

    public async Task<ApiResponse<bool>> UnassignShipmentFromRouteAsync(
        Guid tenantId, Guid routeId, Guid shipmentId)
    {
        var route = await _context.DeliveryRoutes
            .Include(r => r.Stops)
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Id == routeId);

        if (route == null)
            return ApiResponse<bool>.Fail("Route not found");

        var stop = route.Stops.FirstOrDefault(s => s.ShipmentId == shipmentId);
        if (stop == null)
            return ApiResponse<bool>.Fail("Shipment not assigned to this route");

        if (route.Status != RouteStatus.Draft && route.Status != RouteStatus.Planned)
            return ApiResponse<bool>.Fail("Cannot unassign shipment from routes in progress");

        route.Stops.Remove(stop);
        _context.DeliveryStops.Remove(stop);

        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true);
    }

    #endregion

    #region Private Helpers

    private async Task<string> GenerateRouteNumberAsync(Guid tenantId)
    {
        var today = DateTime.UtcNow;
        var prefix = $"RTE-{today:yyyyMMdd}-";

        var lastNumber = await _context.DeliveryRoutes
            .Where(r => r.TenantId == tenantId && r.RouteNumber.StartsWith(prefix))
            .OrderByDescending(r => r.RouteNumber)
            .Select(r => r.RouteNumber)
            .FirstOrDefaultAsync();

        int nextSequence = 1;
        if (lastNumber != null)
        {
            var lastSequence = lastNumber.Substring(prefix.Length);
            if (int.TryParse(lastSequence, out int seq))
                nextSequence = seq + 1;
        }

        return $"{prefix}{nextSequence:D4}";
    }

    private DeliveryStop CreateStopFromRequest(
        Guid tenantId, Guid routeId, int sequence, CreateDeliveryStopRequest request)
    {
        return new DeliveryStop
        {
            TenantId = tenantId,
            RouteId = routeId,
            StopSequence = sequence,
            CustomerName = request.CustomerName,
            AddressLine1 = request.AddressLine1,
            AddressLine2 = request.AddressLine2,
            City = request.City,
            State = request.State,
            PostalCode = request.PostalCode,
            Country = request.Country,
            ContactName = request.ContactName,
            ContactPhone = request.ContactPhone,
            DeliveryInstructions = request.DeliveryInstructions,
            TimeWindowStart = request.TimeWindowStart,
            TimeWindowEnd = request.TimeWindowEnd,
            EstimatedServiceTime = request.EstimatedServiceTime,
            ShipmentId = request.ShipmentId,
            CustomerId = request.CustomerId,
            Notes = request.Notes
        };
    }

    private DeliveryRouteDto MapToDto(DeliveryRoute route)
    {
        return new DeliveryRouteDto(
            route.Id,
            route.RouteNumber,
            route.RouteName,
            route.WarehouseId,
            route.Warehouse?.Name,
            route.CarrierId,
            route.Carrier?.Name,
            route.CarrierServiceId,
            route.CarrierService?.Name,
            route.DriverName,
            route.VehicleNumber,
            route.VehicleType,
            route.PlannedDate,
            route.PlannedDepartureTime,
            route.PlannedReturnTime,
            route.EstimatedStops,
            route.EstimatedDistance,
            route.EstimatedDuration,
            route.ActualDepartureTime,
            route.ActualReturnTime,
            route.Status,
            route.MaxWeight,
            route.MaxVolume,
            route.MaxStops,
            route.CurrentWeight,
            route.CurrentVolume,
            route.Notes,
            route.CreatedAt,
            route.UpdatedAt,
            route.Stops?.Select(MapStopToDto).ToList()
        );
    }

    private DeliveryStopDto MapStopToDto(DeliveryStop stop)
    {
        return new DeliveryStopDto(
            stop.Id,
            stop.RouteId,
            stop.StopSequence,
            stop.CustomerName,
            stop.AddressLine1,
            stop.AddressLine2,
            stop.City,
            stop.State,
            stop.PostalCode,
            stop.Country,
            stop.ContactName,
            stop.ContactPhone,
            stop.DeliveryInstructions,
            stop.TimeWindowStart,
            stop.TimeWindowEnd,
            stop.EstimatedServiceTime,
            stop.ArrivalTime,
            stop.DepartureTime,
            stop.Status,
            stop.SignedBy,
            stop.SignedAt,
            stop.ProofOfDeliveryUrl,
            stop.IssueType,
            stop.IssueNotes,
            stop.ShipmentId,
            stop.Shipment?.ShipmentNumber,
            stop.CustomerId,
            stop.Notes
        );
    }

    #endregion
}
