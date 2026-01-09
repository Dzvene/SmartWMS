namespace SmartWMS.API.Common.Enums;

/// <summary>
/// System permissions for fine-grained authorization.
/// Permissions are assigned to roles and checked via [RequirePermission] attribute.
///
/// ADD PERMISSIONS HERE AS YOU DEVELOP EACH MODULE.
/// Use ranges for organization:
/// - 100-199: Company/Site
/// - 200-299: Users/Roles
/// - 300-399: Warehouse
/// - 400-499: Inventory
/// - 500-599: Sales Orders
/// - 600-699: Purchase Orders
/// - 700-799: Fulfillment
/// - 800-899: Reports
/// - 900-999: System
/// </summary>
public enum Permission
{
    // Permissions will be added during module development
}
