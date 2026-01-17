-- ============================================
-- SmartWMS - Seed Sales Orders Data
-- PostgreSQL
-- ============================================
-- TenantId: e9006ab8-257f-4021-b60a-cbba785bad46
-- WarehouseId: ae5e6339-6212-4435-89a9-f5940815fe20
-- ============================================

-- NOTE: This script requires Customers and Products to exist first.
-- Run seed_customers.sql and ensure products exist before running this script.

DO $$
DECLARE
    v_tenant_id UUID := 'e9006ab8-257f-4021-b60a-cbba785bad46';
    v_warehouse_id UUID := 'ae5e6339-6212-4435-89a9-f5940815fe20';
    v_customer_ids UUID[];
    v_product_ids UUID[];
    v_product_skus TEXT[];
    v_order_id UUID;
BEGIN
    -- Check if orders already exist
    IF EXISTS (SELECT 1 FROM "SalesOrders" WHERE "TenantId" = v_tenant_id LIMIT 1) THEN
        RAISE NOTICE 'Sales orders already exist, skipping seed';
        RETURN;
    END IF;

    -- Get first 8 active customers
    SELECT ARRAY_AGG("Id" ORDER BY "Code")
    INTO v_customer_ids
    FROM (SELECT "Id", "Code" FROM "Customers" WHERE "TenantId" = v_tenant_id AND "IsActive" = true ORDER BY "Code" LIMIT 8) c;

    -- Get first 10 active products
    SELECT ARRAY_AGG("Id" ORDER BY "Sku"), ARRAY_AGG("Sku" ORDER BY "Sku")
    INTO v_product_ids, v_product_skus
    FROM (SELECT "Id", "Sku" FROM "Products" WHERE "TenantId" = v_tenant_id AND "IsActive" = true ORDER BY "Sku" LIMIT 10) p;

    IF array_length(v_customer_ids, 1) < 8 OR array_length(v_product_ids, 1) < 10 THEN
        RAISE NOTICE 'Not enough customers or products, skipping seed';
        RETURN;
    END IF;

    -- Order 1: Pending order
    v_order_id := gen_random_uuid();
    INSERT INTO "SalesOrders" ("Id", "TenantId", "OrderNumber", "CustomerId", "WarehouseId", "Status", "Priority",
        "OrderDate", "RequiredDate", "TotalLines", "TotalQuantity", "Notes", "CreatedAt")
    VALUES (v_order_id, v_tenant_id, 'SO-2024-0001', v_customer_ids[1], v_warehouse_id, 1, 1,
        NOW() - INTERVAL '5 days', NOW() + INTERVAL '7 days', 3, 25, 'Standard delivery', NOW() - INTERVAL '5 days');
    INSERT INTO "SalesOrderLines" ("Id", "TenantId", "OrderId", "LineNumber", "ProductId", "Sku", "QuantityOrdered", "CreatedAt")
    VALUES
        (gen_random_uuid(), v_tenant_id, v_order_id, 1, v_product_ids[1], v_product_skus[1], 10, NOW() - INTERVAL '5 days'),
        (gen_random_uuid(), v_tenant_id, v_order_id, 2, v_product_ids[2], v_product_skus[2], 5, NOW() - INTERVAL '5 days'),
        (gen_random_uuid(), v_tenant_id, v_order_id, 3, v_product_ids[3], v_product_skus[3], 10, NOW() - INTERVAL '5 days');

    -- Order 2: Allocated
    v_order_id := gen_random_uuid();
    INSERT INTO "SalesOrders" ("Id", "TenantId", "OrderNumber", "CustomerId", "WarehouseId", "Status", "Priority",
        "OrderDate", "RequiredDate", "TotalLines", "TotalQuantity", "AllocatedQuantity", "Notes", "CreatedAt")
    VALUES (v_order_id, v_tenant_id, 'SO-2024-0002', v_customer_ids[2], v_warehouse_id, 3, 2,
        NOW() - INTERVAL '3 days', NOW() + INTERVAL '2 days', 2, 15, 15, 'Priority customer - expedite', NOW() - INTERVAL '3 days');
    INSERT INTO "SalesOrderLines" ("Id", "TenantId", "OrderId", "LineNumber", "ProductId", "Sku", "QuantityOrdered", "QuantityAllocated", "CreatedAt")
    VALUES
        (gen_random_uuid(), v_tenant_id, v_order_id, 1, v_product_ids[4], v_product_skus[4], 10, 10, NOW() - INTERVAL '3 days'),
        (gen_random_uuid(), v_tenant_id, v_order_id, 2, v_product_ids[5], v_product_skus[5], 5, 5, NOW() - INTERVAL '3 days');

    -- Order 3: Picked
    v_order_id := gen_random_uuid();
    INSERT INTO "SalesOrders" ("Id", "TenantId", "OrderNumber", "CustomerId", "WarehouseId", "Status", "Priority",
        "OrderDate", "RequiredDate", "TotalLines", "TotalQuantity", "AllocatedQuantity", "PickedQuantity", "CreatedAt")
    VALUES (v_order_id, v_tenant_id, 'SO-2024-0003', v_customer_ids[3], v_warehouse_id, 5, 1,
        NOW() - INTERVAL '4 days', NOW() + INTERVAL '1 day', 1, 20, 20, 20, NOW() - INTERVAL '4 days');
    INSERT INTO "SalesOrderLines" ("Id", "TenantId", "OrderId", "LineNumber", "ProductId", "Sku", "QuantityOrdered", "QuantityAllocated", "QuantityPicked", "CreatedAt")
    VALUES (gen_random_uuid(), v_tenant_id, v_order_id, 1, v_product_ids[6], v_product_skus[6], 20, 20, 20, NOW() - INTERVAL '4 days');

    -- Order 4: Shipped
    v_order_id := gen_random_uuid();
    INSERT INTO "SalesOrders" ("Id", "TenantId", "OrderNumber", "CustomerId", "WarehouseId", "Status", "Priority",
        "OrderDate", "RequiredDate", "ShippedDate", "CarrierCode", "ServiceLevel", "TotalLines", "TotalQuantity",
        "AllocatedQuantity", "PickedQuantity", "ShippedQuantity", "CreatedAt")
    VALUES (v_order_id, v_tenant_id, 'SO-2024-0004', v_customer_ids[4], v_warehouse_id, 7, 1,
        NOW() - INTERVAL '10 days', NOW() - INTERVAL '3 days', NOW() - INTERVAL '4 days', 'DHL', 'Express', 2, 30, 30, 30, 30, NOW() - INTERVAL '10 days');
    INSERT INTO "SalesOrderLines" ("Id", "TenantId", "OrderId", "LineNumber", "ProductId", "Sku", "QuantityOrdered", "QuantityAllocated", "QuantityPicked", "QuantityShipped", "CreatedAt")
    VALUES
        (gen_random_uuid(), v_tenant_id, v_order_id, 1, v_product_ids[7], v_product_skus[7], 15, 15, 15, 15, NOW() - INTERVAL '10 days'),
        (gen_random_uuid(), v_tenant_id, v_order_id, 2, v_product_ids[8], v_product_skus[8], 15, 15, 15, 15, NOW() - INTERVAL '10 days');

    -- Order 5: Delivered
    v_order_id := gen_random_uuid();
    INSERT INTO "SalesOrders" ("Id", "TenantId", "OrderNumber", "CustomerId", "WarehouseId", "Status", "Priority",
        "OrderDate", "RequiredDate", "ShippedDate", "CarrierCode", "ServiceLevel", "TotalLines", "TotalQuantity",
        "AllocatedQuantity", "PickedQuantity", "ShippedQuantity", "CreatedAt")
    VALUES (v_order_id, v_tenant_id, 'SO-2024-0005', v_customer_ids[5], v_warehouse_id, 8, 0,
        NOW() - INTERVAL '15 days', NOW() - INTERVAL '8 days', NOW() - INTERVAL '9 days', 'FEDEX', 'Ground', 3, 50, 50, 50, 50, NOW() - INTERVAL '15 days');
    INSERT INTO "SalesOrderLines" ("Id", "TenantId", "OrderId", "LineNumber", "ProductId", "Sku", "QuantityOrdered", "QuantityAllocated", "QuantityPicked", "QuantityShipped", "CreatedAt")
    VALUES
        (gen_random_uuid(), v_tenant_id, v_order_id, 1, v_product_ids[1], v_product_skus[1], 20, 20, 20, 20, NOW() - INTERVAL '15 days'),
        (gen_random_uuid(), v_tenant_id, v_order_id, 2, v_product_ids[2], v_product_skus[2], 15, 15, 15, 15, NOW() - INTERVAL '15 days'),
        (gen_random_uuid(), v_tenant_id, v_order_id, 3, v_product_ids[3], v_product_skus[3], 15, 15, 15, 15, NOW() - INTERVAL '15 days');

    -- Order 6: Confirmed - Urgent
    v_order_id := gen_random_uuid();
    INSERT INTO "SalesOrders" ("Id", "TenantId", "OrderNumber", "CustomerId", "WarehouseId", "Status", "Priority",
        "OrderDate", "RequiredDate", "CarrierCode", "ServiceLevel", "TotalLines", "TotalQuantity",
        "Notes", "InternalNotes", "CreatedAt")
    VALUES (v_order_id, v_tenant_id, 'SO-2024-0006', v_customer_ids[6], v_warehouse_id, 2, 3,
        NOW() - INTERVAL '1 day', NOW(), 'UPS', 'Next Day Air', 1, 5,
        'URGENT - Customer escalation', 'CEO personal order - handle with care', NOW() - INTERVAL '1 day');
    INSERT INTO "SalesOrderLines" ("Id", "TenantId", "OrderId", "LineNumber", "ProductId", "Sku", "QuantityOrdered", "CreatedAt")
    VALUES (gen_random_uuid(), v_tenant_id, v_order_id, 1, v_product_ids[4], v_product_skus[4], 5, NOW() - INTERVAL '1 day');

    -- Order 7: On Hold
    v_order_id := gen_random_uuid();
    INSERT INTO "SalesOrders" ("Id", "TenantId", "OrderNumber", "CustomerId", "WarehouseId", "Status", "Priority",
        "OrderDate", "RequiredDate", "TotalLines", "TotalQuantity", "Notes", "CreatedAt")
    VALUES (v_order_id, v_tenant_id, 'SO-2024-0007', v_customer_ids[7], v_warehouse_id, 10, 1,
        NOW() - INTERVAL '7 days', NOW() + INTERVAL '5 days', 2, 100, 'Awaiting payment confirmation', NOW() - INTERVAL '7 days');
    INSERT INTO "SalesOrderLines" ("Id", "TenantId", "OrderId", "LineNumber", "ProductId", "Sku", "QuantityOrdered", "CreatedAt")
    VALUES
        (gen_random_uuid(), v_tenant_id, v_order_id, 1, v_product_ids[9], v_product_skus[9], 50, NOW() - INTERVAL '7 days'),
        (gen_random_uuid(), v_tenant_id, v_order_id, 2, v_product_ids[10], v_product_skus[10], 50, NOW() - INTERVAL '7 days');

    -- Order 8: Cancelled
    v_order_id := gen_random_uuid();
    INSERT INTO "SalesOrders" ("Id", "TenantId", "OrderNumber", "CustomerId", "WarehouseId", "Status", "Priority",
        "OrderDate", "RequiredDate", "TotalLines", "TotalQuantity", "Notes", "CreatedAt")
    VALUES (v_order_id, v_tenant_id, 'SO-2024-0008', v_customer_ids[8], v_warehouse_id, 9, 1,
        NOW() - INTERVAL '12 days', NOW() - INTERVAL '5 days', 1, 10, 'Cancelled by customer request', NOW() - INTERVAL '12 days');
    INSERT INTO "SalesOrderLines" ("Id", "TenantId", "OrderId", "LineNumber", "ProductId", "Sku", "QuantityOrdered", "QuantityCancelled", "CreatedAt")
    VALUES (gen_random_uuid(), v_tenant_id, v_order_id, 1, v_product_ids[5], v_product_skus[5], 10, 10, NOW() - INTERVAL '12 days');

    -- Order 9: Draft
    v_order_id := gen_random_uuid();
    INSERT INTO "SalesOrders" ("Id", "TenantId", "OrderNumber", "CustomerId", "WarehouseId", "Status", "Priority",
        "OrderDate", "RequiredDate", "TotalLines", "TotalQuantity", "Notes", "CreatedAt")
    VALUES (v_order_id, v_tenant_id, 'SO-2024-0009', v_customer_ids[1], v_warehouse_id, 0, 1,
        NOW(), NOW() + INTERVAL '14 days', 4, 40, 'Draft - pending review', NOW());
    INSERT INTO "SalesOrderLines" ("Id", "TenantId", "OrderId", "LineNumber", "ProductId", "Sku", "QuantityOrdered", "CreatedAt")
    VALUES
        (gen_random_uuid(), v_tenant_id, v_order_id, 1, v_product_ids[1], v_product_skus[1], 10, NOW()),
        (gen_random_uuid(), v_tenant_id, v_order_id, 2, v_product_ids[2], v_product_skus[2], 10, NOW()),
        (gen_random_uuid(), v_tenant_id, v_order_id, 3, v_product_ids[3], v_product_skus[3], 10, NOW()),
        (gen_random_uuid(), v_tenant_id, v_order_id, 4, v_product_ids[4], v_product_skus[4], 10, NOW());

    -- Order 10: Packed
    v_order_id := gen_random_uuid();
    INSERT INTO "SalesOrders" ("Id", "TenantId", "OrderNumber", "CustomerId", "WarehouseId", "Status", "Priority",
        "OrderDate", "RequiredDate", "CarrierCode", "ServiceLevel", "TotalLines", "TotalQuantity",
        "AllocatedQuantity", "PickedQuantity", "Notes", "CreatedAt")
    VALUES (v_order_id, v_tenant_id, 'SO-2024-0010', v_customer_ids[2], v_warehouse_id, 6, 2,
        NOW() - INTERVAL '2 days', NOW() + INTERVAL '1 day', 'DHL', 'Express', 2, 12, 12, 12, 'Ready for dispatch', NOW() - INTERVAL '2 days');
    INSERT INTO "SalesOrderLines" ("Id", "TenantId", "OrderId", "LineNumber", "ProductId", "Sku", "QuantityOrdered", "QuantityAllocated", "QuantityPicked", "CreatedAt")
    VALUES
        (gen_random_uuid(), v_tenant_id, v_order_id, 1, v_product_ids[6], v_product_skus[6], 6, 6, 6, NOW() - INTERVAL '2 days'),
        (gen_random_uuid(), v_tenant_id, v_order_id, 2, v_product_ids[7], v_product_skus[7], 6, 6, 6, NOW() - INTERVAL '2 days');

    RAISE NOTICE 'Successfully inserted 10 sales orders with lines';
END $$;

-- Status enum values reference:
-- 0 = Draft, 1 = Pending, 2 = Confirmed, 3 = Allocated, 4 = PartiallyPicked
-- 5 = Picked, 6 = Packed, 7 = Shipped, 8 = Delivered, 9 = Cancelled, 10 = OnHold

-- Priority enum values reference:
-- 0 = Low, 1 = Normal, 2 = High, 3 = Urgent

-- Useful queries:
-- SELECT * FROM "SalesOrders" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
-- SELECT so.*, sol.* FROM "SalesOrders" so JOIN "SalesOrderLines" sol ON so."Id" = sol."OrderId" WHERE so."TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
