using SmartWMS.API.Modules.Returns.Models;

namespace SmartWMS.API.Modules.Returns.DTOs;

#region Return Order DTOs

public class ReturnOrderDto
{
    public Guid Id { get; set; }
    public required string ReturnNumber { get; set; }

    public Guid? OriginalSalesOrderId { get; set; }
    public string? OriginalSalesOrderNumber { get; set; }

    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }

    public ReturnOrderStatus Status { get; set; }
    public ReturnType ReturnType { get; set; }

    public Guid? ReasonCodeId { get; set; }
    public string? ReasonDescription { get; set; }

    public Guid? ReceivingLocationId { get; set; }
    public string? ReceivingLocationCode { get; set; }

    public DateTime? RequestedDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public DateTime? ProcessedDate { get; set; }

    public string? RmaNumber { get; set; }
    public DateTime? RmaExpiryDate { get; set; }

    public string? CarrierCode { get; set; }
    public string? TrackingNumber { get; set; }

    public Guid? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }

    public int TotalLines { get; set; }
    public decimal TotalQuantityExpected { get; set; }
    public decimal TotalQuantityReceived { get; set; }
    public decimal ReceivingProgress => TotalQuantityExpected > 0
        ? TotalQuantityReceived / TotalQuantityExpected * 100
        : 0;

    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }

    public List<ReturnOrderLineDto> Lines { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ReturnOrderListDto
{
    public Guid Id { get; set; }
    public required string ReturnNumber { get; set; }
    public string? OriginalSalesOrderNumber { get; set; }
    public string? CustomerName { get; set; }
    public ReturnOrderStatus Status { get; set; }
    public ReturnType ReturnType { get; set; }
    public string? RmaNumber { get; set; }
    public int TotalLines { get; set; }
    public decimal TotalQuantityExpected { get; set; }
    public decimal TotalQuantityReceived { get; set; }
    public DateTime? RequestedDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

#endregion

#region Return Order Line DTOs

public class ReturnOrderLineDto
{
    public Guid Id { get; set; }
    public Guid ReturnOrderId { get; set; }
    public int LineNumber { get; set; }

    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public required string Sku { get; set; }

    public decimal QuantityExpected { get; set; }
    public decimal QuantityReceived { get; set; }
    public decimal QuantityAccepted { get; set; }
    public decimal QuantityRejected { get; set; }

    public ReturnCondition Condition { get; set; }
    public string? ConditionNotes { get; set; }

    public ReturnDisposition Disposition { get; set; }
    public Guid? DispositionLocationId { get; set; }
    public string? DispositionLocationCode { get; set; }

    public string? BatchNumber { get; set; }
    public string? SerialNumber { get; set; }

    public Guid? OriginalOrderLineId { get; set; }

    public Guid? ReasonCodeId { get; set; }
    public string? ReasonDescription { get; set; }

    public string? Notes { get; set; }
}

#endregion

#region Request DTOs

public class CreateReturnOrderRequest
{
    public Guid? OriginalSalesOrderId { get; set; }
    public Guid CustomerId { get; set; }
    public ReturnType ReturnType { get; set; } = ReturnType.CustomerReturn;
    public Guid? ReasonCodeId { get; set; }
    public string? ReasonDescription { get; set; }
    public Guid? ReceivingLocationId { get; set; }
    public DateTime? RequestedDate { get; set; }
    public string? RmaNumber { get; set; }
    public DateTime? RmaExpiryDate { get; set; }
    public string? Notes { get; set; }
    public List<CreateReturnOrderLineRequest> Lines { get; set; } = new();
}

public class CreateReturnOrderLineRequest
{
    public Guid ProductId { get; set; }
    public required string Sku { get; set; }
    public decimal QuantityExpected { get; set; }
    public string? BatchNumber { get; set; }
    public string? SerialNumber { get; set; }
    public Guid? OriginalOrderLineId { get; set; }
    public Guid? ReasonCodeId { get; set; }
    public string? ReasonDescription { get; set; }
    public string? Notes { get; set; }
}

public class UpdateReturnOrderRequest
{
    public Guid? ReasonCodeId { get; set; }
    public string? ReasonDescription { get; set; }
    public Guid? ReceivingLocationId { get; set; }
    public string? RmaNumber { get; set; }
    public DateTime? RmaExpiryDate { get; set; }
    public string? CarrierCode { get; set; }
    public string? TrackingNumber { get; set; }
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }
}

public class AddReturnLineRequest
{
    public Guid ProductId { get; set; }
    public required string Sku { get; set; }
    public decimal QuantityExpected { get; set; }
    public string? BatchNumber { get; set; }
    public string? SerialNumber { get; set; }
    public Guid? ReasonCodeId { get; set; }
    public string? ReasonDescription { get; set; }
    public string? Notes { get; set; }
}

public class ReceiveReturnLineRequest
{
    public decimal QuantityReceived { get; set; }
    public ReturnCondition Condition { get; set; }
    public string? ConditionNotes { get; set; }
    public string? BatchNumber { get; set; }
    public string? SerialNumber { get; set; }
}

public class ProcessReturnLineRequest
{
    public decimal QuantityAccepted { get; set; }
    public decimal QuantityRejected { get; set; }
    public ReturnDisposition Disposition { get; set; }
    public Guid? DispositionLocationId { get; set; }
    public string? Notes { get; set; }
}

public class AssignReturnRequest
{
    public Guid UserId { get; set; }
}

public class SetReturnShippingRequest
{
    public string? CarrierCode { get; set; }
    public required string TrackingNumber { get; set; }
}

#endregion

#region Filter DTOs

public class ReturnOrderFilters
{
    public ReturnOrderStatus? Status { get; set; }
    public ReturnType? ReturnType { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? OriginalSalesOrderId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? RmaNumber { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

#endregion
