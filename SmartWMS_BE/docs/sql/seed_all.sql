-- ============================================
-- SmartWMS - Master Seed Script
-- PostgreSQL
-- ============================================
-- This script runs all seed files in the correct order
-- TenantId: e9006ab8-257f-4021-b60a-cbba785bad46
-- ============================================
-- IMPORTANT: Run scripts in this exact order due to dependencies
-- ============================================

-- Phase 1: Foundation (no dependencies)
\echo '=== Phase 1: Foundation ==='
\i seed_company.sql
\i seed_users_roles.sql
\i seed_sites.sql

-- Phase 2: Warehouse Structure
\echo '=== Phase 2: Warehouse Structure ==='
\i seed_warehouses.sql
\i seed_zones.sql
\i seed_locations.sql
\i seed_equipment.sql

-- Phase 3: Configuration
\echo '=== Phase 3: Configuration ==='
\i seed_configuration.sql
\i seed_carriers.sql

-- Phase 4: Master Data
\echo '=== Phase 4: Master Data ==='
\i seed_product_categories.sql
\i seed_products.sql
\i seed_customers.sql
\i seed_suppliers.sql

-- Phase 5: Inventory
\echo '=== Phase 5: Inventory ==='
\i seed_stock_levels.sql
\i seed_stock_movements.sql

-- Phase 6: Orders
\echo '=== Phase 6: Orders ==='
\i seed_purchase_orders.sql
\i seed_sales_orders.sql

-- Phase 7: Inbound Operations
\echo '=== Phase 7: Inbound Operations ==='
\i seed_goods_receipts.sql
\i seed_putaway_tasks.sql
\i seed_returns.sql

-- Phase 8: Outbound Operations
\echo '=== Phase 8: Outbound Operations ==='
\i seed_fulfillment.sql
\i seed_packing.sql
\i seed_shipments.sql

-- Phase 9: Inventory Management
\echo '=== Phase 9: Inventory Management ==='
\i seed_adjustments_transfers.sql
\i seed_cycle_count.sql

-- Phase 10: Supporting Systems
\echo '=== Phase 10: Supporting Systems ==='
\i seed_notifications.sql
\i seed_integrations.sql
\i seed_automation.sql

-- Phase 11: Audit & Sessions
\echo '=== Phase 11: Audit & Sessions ==='
\i seed_audit_logs.sql
\i seed_sessions_operations.sql

\echo '=== All seed scripts completed successfully ==='

-- ============================================
-- Alternative: Run individual scripts manually
-- ============================================
-- psql -d smartwms -f seed_company.sql
-- psql -d smartwms -f seed_users_roles.sql
-- ... etc
-- ============================================

-- ============================================
-- Verification queries after seeding:
-- ============================================
-- SELECT 'Companies' AS "Table", COUNT(*) AS "Count" FROM "Companies"
-- UNION ALL SELECT 'Users', COUNT(*) FROM "Users"
-- UNION ALL SELECT 'Roles', COUNT(*) FROM "Roles"
-- UNION ALL SELECT 'Sites', COUNT(*) FROM "Sites"
-- UNION ALL SELECT 'Warehouses', COUNT(*) FROM "Warehouses"
-- UNION ALL SELECT 'Zones', COUNT(*) FROM "Zones"
-- UNION ALL SELECT 'Locations', COUNT(*) FROM "Locations"
-- UNION ALL SELECT 'Products', COUNT(*) FROM "Products"
-- UNION ALL SELECT 'Customers', COUNT(*) FROM "Customers"
-- UNION ALL SELECT 'Suppliers', COUNT(*) FROM "Suppliers"
-- UNION ALL SELECT 'SalesOrders', COUNT(*) FROM "SalesOrders"
-- UNION ALL SELECT 'PurchaseOrders', COUNT(*) FROM "PurchaseOrders"
-- UNION ALL SELECT 'Equipment', COUNT(*) FROM "Equipment"
-- UNION ALL SELECT 'StockLevels', COUNT(*) FROM "StockLevels"
-- ORDER BY "Table";
