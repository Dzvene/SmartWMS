using SmartWMS.API.Common.Models;

namespace SmartWMS.API.Modules.Fulfillment.Models;

/// <summary>
/// FulfillmentOrder - links a sales order to a fulfillment batch.
/// Allows a single order to be split across multiple batches if needed.
/// </summary>
public class FulfillmentOrder : TenantEntity
{
    // Batch reference
    public Guid BatchId { get; set; }

    // Order reference
    public Guid OrderId { get; set; }

    // Sequence within batch
    public int Sequence { get; set; }

    // Navigation properties
    public virtual FulfillmentBatch Batch { get; set; } = null!;
    public virtual Orders.Models.SalesOrder Order { get; set; } = null!;
}
