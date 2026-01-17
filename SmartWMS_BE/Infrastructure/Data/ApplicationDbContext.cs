using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartWMS.API.Infrastructure.Identity;
using SmartWMS.API.Modules.Companies.Models;
using SmartWMS.API.Modules.Sites.Models;
using SmartWMS.API.Modules.Warehouse.Models;
using SmartWMS.API.Modules.Inventory.Models;
using SmartWMS.API.Modules.Orders.Models;
using SmartWMS.API.Modules.Fulfillment.Models;
using SmartWMS.API.Modules.Equipment.Models;
using SmartWMS.API.Modules.Receiving.Models;
using SmartWMS.API.Modules.Putaway.Models;
using SmartWMS.API.Modules.Packing.Models;
using SmartWMS.API.Modules.Returns.Models;
using SmartWMS.API.Modules.Carriers.Models;
using SmartWMS.API.Modules.CycleCount.Models;
using SmartWMS.API.Modules.Configuration.Models;
using SmartWMS.API.Modules.Adjustments.Models;
using SmartWMS.API.Modules.Transfers.Models;
using SmartWMS.API.Modules.Shipping.Models;
using SmartWMS.API.Modules.Audit.Models;
using SmartWMS.API.Modules.Notifications.Models;
using SmartWMS.API.Modules.Integrations.Models;
using SmartWMS.API.Modules.Sessions.Models;
using SmartWMS.API.Modules.OperationHub.Models;
using SmartWMS.API.Modules.Automation.Models;

namespace SmartWMS.API.Infrastructure.Data;

/// <summary>
/// Main database context for SmartWMS application.
/// Extends IdentityDbContext for ASP.NET Identity support with custom user/role.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Company & Multi-tenancy
    public DbSet<Company> Companies => Set<Company>();

    // Sites
    public DbSet<Site> Sites => Set<Site>();

    // Warehouse
    public DbSet<Modules.Warehouse.Models.Warehouse> Warehouses => Set<Modules.Warehouse.Models.Warehouse>();
    public DbSet<Zone> Zones => Set<Zone>();
    public DbSet<Location> Locations => Set<Location>();

    // Inventory
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<StockLevel> StockLevels => Set<StockLevel>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    // Orders
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<SalesOrderLine> SalesOrderLines => Set<SalesOrderLine>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();

    // Fulfillment
    public DbSet<FulfillmentBatch> FulfillmentBatches => Set<FulfillmentBatch>();
    public DbSet<FulfillmentOrder> FulfillmentOrders => Set<FulfillmentOrder>();
    public DbSet<PickTask> PickTasks => Set<PickTask>();
    public DbSet<Shipment> Shipments => Set<Shipment>();

    // Equipment
    public DbSet<Modules.Equipment.Models.Equipment> Equipment => Set<Modules.Equipment.Models.Equipment>();

    // Receiving
    public DbSet<GoodsReceipt> GoodsReceipts => Set<GoodsReceipt>();
    public DbSet<GoodsReceiptLine> GoodsReceiptLines => Set<GoodsReceiptLine>();

    // Putaway
    public DbSet<PutawayTask> PutawayTasks => Set<PutawayTask>();

    // Packing
    public DbSet<PackingTask> PackingTasks => Set<PackingTask>();
    public DbSet<Package> Packages => Set<Package>();
    public DbSet<PackageItem> PackageItems => Set<PackageItem>();
    public DbSet<PackingStation> PackingStations => Set<PackingStation>();

    // Returns
    public DbSet<ReturnOrder> ReturnOrders => Set<ReturnOrder>();
    public DbSet<ReturnOrderLine> ReturnOrderLines => Set<ReturnOrderLine>();

    // Carriers
    public DbSet<Carrier> Carriers => Set<Carrier>();
    public DbSet<CarrierService> CarrierServices => Set<CarrierService>();

    // Cycle Count
    public DbSet<CycleCountSession> CycleCountSessions => Set<CycleCountSession>();
    public DbSet<CycleCountItem> CycleCountItems => Set<CycleCountItem>();

    // Configuration
    public DbSet<BarcodePrefix> BarcodePrefixes => Set<BarcodePrefix>();
    public DbSet<ReasonCode> ReasonCodes => Set<ReasonCode>();
    public DbSet<NumberSequence> NumberSequences => Set<NumberSequence>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    // Adjustments
    public DbSet<StockAdjustment> StockAdjustments => Set<StockAdjustment>();
    public DbSet<StockAdjustmentLine> StockAdjustmentLines => Set<StockAdjustmentLine>();

    // Transfers
    public DbSet<StockTransfer> StockTransfers => Set<StockTransfer>();
    public DbSet<StockTransferLine> StockTransferLines => Set<StockTransferLine>();

    // Shipping/Deliveries
    public DbSet<DeliveryRoute> DeliveryRoutes => Set<DeliveryRoute>();
    public DbSet<DeliveryStop> DeliveryStops => Set<DeliveryStop>();
    public DbSet<DeliveryTrackingEvent> DeliveryTrackingEvents => Set<DeliveryTrackingEvent>();

    // Audit/Logs
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<SystemEventLog> SystemEventLogs => Set<SystemEventLog>();

    // Notifications
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();

    // Integrations
    public DbSet<Integration> Integrations => Set<Integration>();
    public DbSet<IntegrationLog> IntegrationLogs => Set<IntegrationLog>();
    public DbSet<IntegrationMapping> IntegrationMappings => Set<IntegrationMapping>();
    public DbSet<WebhookEndpoint> WebhookEndpoints => Set<WebhookEndpoint>();
    public DbSet<SyncJob> SyncJobs => Set<SyncJob>();

    // Sessions
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<LoginAttempt> LoginAttempts => Set<LoginAttempt>();
    public DbSet<TrustedDevice> TrustedDevices => Set<TrustedDevice>();

    // Operation Hub
    public DbSet<OperatorSession> OperatorSessions => Set<OperatorSession>();
    public DbSet<OperatorProductivity> OperatorProductivity => Set<OperatorProductivity>();
    public DbSet<ScanLog> ScanLogs => Set<ScanLog>();
    public DbSet<TaskActionLog> TaskActionLogs => Set<TaskActionLog>();

    // Automation
    public DbSet<AutomationRule> AutomationRules => Set<AutomationRule>();
    public DbSet<RuleCondition> RuleConditions => Set<RuleCondition>();
    public DbSet<RuleExecution> RuleExecutions => Set<RuleExecution>();
    public DbSet<ActionTemplate> ActionTemplates => Set<ActionTemplate>();
    public DbSet<ScheduledJob> ScheduledJobs => Set<ScheduledJob>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply all configurations from assembly
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Configure Identity tables with custom schema/names if needed
        builder.Entity<ApplicationUser>(b =>
        {
            b.ToTable("Users");
        });

        builder.Entity<ApplicationRole>(b =>
        {
            b.ToTable("Roles");
        });

        // Global query filter for soft-delete (if needed in future)
        // builder.Entity<TenantEntity>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is Common.Models.BaseEntity &&
                       (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (Common.Models.BaseEntity)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
