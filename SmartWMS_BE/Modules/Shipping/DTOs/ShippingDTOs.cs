using SmartWMS.API.Modules.Shipping.Models;

namespace SmartWMS.API.Modules.Shipping.DTOs;

#region Delivery Route DTOs

public record DeliveryRouteDto(
    Guid Id,
    string RouteNumber,
    string? RouteName,
    Guid WarehouseId,
    string? WarehouseName,
    Guid? CarrierId,
    string? CarrierName,
    Guid? CarrierServiceId,
    string? CarrierServiceName,
    string? DriverName,
    string? VehicleNumber,
    string? VehicleType,
    DateTime PlannedDate,
    DateTime? PlannedDepartureTime,
    DateTime? PlannedReturnTime,
    int? EstimatedStops,
    decimal? EstimatedDistance,
    int? EstimatedDuration,
    DateTime? ActualDepartureTime,
    DateTime? ActualReturnTime,
    RouteStatus Status,
    decimal? MaxWeight,
    decimal? MaxVolume,
    int? MaxStops,
    decimal CurrentWeight,
    decimal CurrentVolume,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<DeliveryStopDto>? Stops
);

public record DeliveryStopDto(
    Guid Id,
    Guid RouteId,
    int StopSequence,
    string? CustomerName,
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? State,
    string? PostalCode,
    string? Country,
    string? ContactName,
    string? ContactPhone,
    string? DeliveryInstructions,
    DateTime? TimeWindowStart,
    DateTime? TimeWindowEnd,
    int? EstimatedServiceTime,
    DateTime? ArrivalTime,
    DateTime? DepartureTime,
    StopStatus Status,
    string? SignedBy,
    DateTime? SignedAt,
    string? ProofOfDeliveryUrl,
    DeliveryIssue? IssueType,
    string? IssueNotes,
    Guid? ShipmentId,
    string? ShipmentNumber,
    Guid? CustomerId,
    string? Notes
);

public record DeliveryRouteSummaryDto(
    Guid Id,
    string RouteNumber,
    string? RouteName,
    string? WarehouseName,
    string? CarrierName,
    string? DriverName,
    DateTime PlannedDate,
    RouteStatus Status,
    int StopCount,
    int CompletedStops,
    DateTime CreatedAt
);

public record DeliveryTrackingEventDto(
    Guid Id,
    Guid RouteId,
    Guid? StopId,
    Guid? ShipmentId,
    TrackingEventType EventType,
    DateTime EventTime,
    string? EventDescription,
    decimal? Latitude,
    decimal? Longitude,
    string? LocationDescription,
    string? PerformedBy,
    string? Notes
);

#endregion

#region Create/Update Requests

public record CreateDeliveryRouteRequest(
    string? RouteName,
    Guid WarehouseId,
    Guid? CarrierId,
    Guid? CarrierServiceId,
    string? DriverName,
    string? VehicleNumber,
    string? VehicleType,
    DateTime PlannedDate,
    DateTime? PlannedDepartureTime,
    DateTime? PlannedReturnTime,
    decimal? MaxWeight,
    decimal? MaxVolume,
    int? MaxStops,
    string? Notes,
    List<CreateDeliveryStopRequest>? Stops
);

public record CreateDeliveryStopRequest(
    string? CustomerName,
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? State,
    string? PostalCode,
    string? Country,
    string? ContactName,
    string? ContactPhone,
    string? DeliveryInstructions,
    DateTime? TimeWindowStart,
    DateTime? TimeWindowEnd,
    int? EstimatedServiceTime,
    Guid? ShipmentId,
    Guid? CustomerId,
    string? Notes
);

public record UpdateDeliveryRouteRequest(
    string? RouteName,
    Guid? CarrierId,
    Guid? CarrierServiceId,
    string? DriverName,
    string? VehicleNumber,
    string? VehicleType,
    DateTime? PlannedDate,
    DateTime? PlannedDepartureTime,
    DateTime? PlannedReturnTime,
    decimal? MaxWeight,
    decimal? MaxVolume,
    int? MaxStops,
    string? Notes
);

public record UpdateDeliveryStopRequest(
    int? StopSequence,
    string? CustomerName,
    string? AddressLine1,
    string? City,
    string? ContactPhone,
    string? DeliveryInstructions,
    DateTime? TimeWindowStart,
    DateTime? TimeWindowEnd,
    string? Notes
);

#endregion

#region Action Requests

public record ReleaseRouteRequest(
    string? Notes
);

public record StartLoadingRequest(
    string? Notes
);

public record CompleteLoadingRequest(
    decimal? ActualWeight,
    decimal? ActualVolume,
    string? Notes
);

public record DepartRouteRequest(
    DateTime? DepartureTime
);

public record ArriveAtStopRequest(
    DateTime? ArrivalTime,
    decimal? Latitude,
    decimal? Longitude
);

public record CompleteStopDeliveryRequest(
    string? SignedBy,
    string? ProofOfDeliveryUrl,
    DeliveryIssue? IssueType,
    string? IssueNotes,
    bool IsPartial = false
);

public record FailStopDeliveryRequest(
    DeliveryIssue IssueType,
    string IssueNotes
);

public record ReturnToWarehouseRequest(
    DateTime? ReturnTime,
    string? Notes
);

public record AddTrackingEventRequest(
    Guid? StopId,
    Guid? ShipmentId,
    TrackingEventType EventType,
    string? EventDescription,
    decimal? Latitude,
    decimal? Longitude,
    string? LocationDescription,
    string? Notes
);

#endregion

#region Query/Filter

public record RouteFilterRequest(
    Guid? WarehouseId = null,
    Guid? CarrierId = null,
    RouteStatus? Status = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null,
    string? DriverName = null,
    string? SearchTerm = null,
    int Page = 1,
    int PageSize = 20
);

#endregion

#region Auto-Planning

public record AutoPlanRouteRequest(
    Guid WarehouseId,
    DateTime PlannedDate,
    List<Guid> ShipmentIds,
    Guid? CarrierId,
    decimal? MaxWeight,
    decimal? MaxVolume,
    int? MaxStops
);

public record OptimizeRouteRequest(
    bool MinimizeDistance = true,
    bool RespectTimeWindows = true
);

#endregion
