-- ============================================
-- SmartWMS - Seed Purchase Orders Data
-- PostgreSQL
-- ============================================
-- TenantId: e9006ab8-257f-4021-b60a-cbba785bad46
-- WarehouseId: ae5e6339-6212-4435-89a9-f5940815fe20
-- ============================================

-- NOTE: This script requires Suppliers and Products to exist first.
-- Run seed_suppliers.sql and ensure products exist before running this script.

DO $$
DECLARE
    v_tenant_id UUID := 'e9006ab8-257f-4021-b60a-cbba785bad46';
    v_warehouse_id UUID := 'ae5e6339-6212-4435-89a9-f5940815fe20';
    v_supplier_ids UUID[];
    v_product_ids UUID[];
    v_product_skus TEXT[];
    v_order_id UUID;
BEGIN
    -- Check if orders already exist
    IF EXISTS (SELECT 1 FROM "PurchaseOrders" WHERE "TenantId" = v_tenant_id LIMIT 1) THEN
        RAISE NOTICE 'Purchase orders already exist, skipping seed';
        RETURN;
    END IF;

    -- Get first 8 active suppliers
    SELECT ARRAY_AGG("Id" ORDER BY "Code")
    INTO v_supplier_ids
    FROM (SELECT "Id", "Code" FROM "Suppliers" WHERE "TenantId" = v_tenant_id AND "IsActive" = true ORDER BY "Code" LIMIT 8) s;

    -- Get first 10 active products
    SELECT ARRAY_AGG("Id" ORDER BY "Sku"), ARRAY_AGG("Sku" ORDER BY "Sku")
    INTO v_product_ids, v_product_skus
    FROM (SELECT "Id", "Sku" FROM "Products" WHERE "TenantId" = v_tenant_id AND "IsActive" = true ORDER BY "Sku" LIMIT 10) p;

    IF array_length(v_supplier_ids, 1) < 8 OR array_length(v_product_ids, 1) < 10 THEN
        RAISE NOTICE 'Not enough suppliers or products, skipping seed';
        RETURN;
    END IF;

    -- Order 1: Draft
    v_order_id := gen_random_uuid();
    INSERT INTO "PurchaseOrders" ("Id", "TenantId", "OrderNumber", "SupplierId", "WarehouseId", "Status",
        "OrderDate", "ExpectedDate", "TotalLines", "TotalQuantity", "ReceivedQuantity", "Notes", "CreatedAt")
    VALUES (v_order_id, v_tenant_id, 'PO-2024-0001', v_supplier_ids[1], v_warehouse_id, 0,
        NOW(), NOW() + INTERVAL '14 days', 3, 150, 0, 'Draft - awaiting approval', NOW());
    INSERT INTO "PurchaseOrderLines" ("Id", "TenantId", "OrderId", "LineNumber", "ProductId", "Sku", "QuantityOrdered", "QuantityReceived", "QuantityCancelled", "CreatedAt")
    VALUES
        (gen_random_uuid(), v_tenant_id, v_order_id, 1, v_product_ids[1], v_product_skus[1], 50, 0, 0, NOW()),
        (gen_random_uuid(), v_tenant_id, v_order_id, 2, v_product_ids[2], v_product_skus[2], 50, 0, 0, NOW()),
        (gen_random_uuid(), v_tenant_id, v_order_id, 3, v_product_ids[3], v_product_skus[3], 50, 0, 0, NOW());

    -- Order 2: Pending
    v_order_id := gen_random_uuid();
    INSERT INTO "PurchaseOrders" ("Id", "TenantId", "OrderNumber", "SupplierId", "WarehouseId", "Status",
        "OrderDate", "ExpectedDate", "TotalLines", "TotalQuantity", "ReceivedQuantity", "Notes", "CreatedAt")
    VALUES (v_order_id, v_tenant_id, 'PO-2024-0002', v_supplier_ids[2], v_warehouse_id, 1,
        NOW() - INTERVAL '2 days', NOW() + INTERVAL '12 days', 2, 100, 0, 'Sent to supplier - awaiting confirmation', NOW() - INTERVAL '2 days');
    INSERT INTO "PurchaseOrderLines" ("Id", "TenantId", "OrderId", "LineNumber", "ProductId", "Sku", "QuantityOrdered", "QuantityReceived", "QuantityCancelled", "CreatedAt")
    VALUES
        (gen_random_uuid(), v_tenant_id, v_order_id, 1, v_product_ids[4], v_product_skus[4], 60, 0, 0, NOW() - INTERVAL '2 days'),
        (gen_random_uuid(), v_tenant_id, v_order_id, 2, v_product_ids[5], v_product_skus[5], 40, 0, 0, NOW() - INTERVAL '2 days');

    -- Order 3: Confirmed - awaiting delivery
    v_order_id := gen_random_uuid();
    INSERT INTO "PurchaseOrders" ("Id", "TenantId", "OrderNumber", "SupplierId", "WarehouseId", "Status",
        "OrderDate", "ExpectedDate", "TotalLines", "TotalQuantity", "ReceivedQuantity", "Notes", "InternalNotes", "CreatedAt")
    VALUES (v_order_id, v_tenant_id, 'PO-2024-0003', v_supplier_ids[3], v_warehouse_id, 2,
        NOW() - INTERVAL '5 days', NOW() + INTERVAL '5 days', 1, 200, 0, 'Confirmed by supplier', 'Schedule receiving dock B', NOW() - INTERVAL '5 days');
    INSERT INTO "PurchaseOrderLines" ("Id", "TenantId", "OrderId", "LineNumber", "ProductId", "Sku", "QuantityOrdered", "QuantityReceived", "QuantityCancelled", "ExpectedBatchNumber", "CreatedAt")
    VALUES (gen_random_uuid(), v_tenant_id, v_order_id, 1, v_product_ids[6], v_product_skus[6], 200, 0, 0, 'BATCH-2024-001', NOW() - INTERVAL '5 days');

    -- Order 4: Partially Received
    v_order_id := gen_random_uuid();
    INSERT INTO "PurchaseOrders" ("Id", "TenantId", "OrderNumber", "SupplierId", "WarehouseId", "Status",
        "OrderDate", "ExpectedDate", "TotalLines", "TotalQuantity", "ReceivedQuantity", "Notes", "CreatedAt")
    VALUES (v_order_id, v_tenant_id, 'PO-2024-0004', v_supplier_ids[4], v_warehouse_id, 3,
        NOW() - INTERVAL '10 days', NOW() - INTERVAL '3 days', 2, 300, 150, 'First shipment received, awaiting remainder', NOW() - INTERVAL '10 days');
    INSERT INTO "PurchaseOrderLines" ("Id", "TenantId", "OrderId", "LineNumber", "ProductId", "Sku", "QuantityOrdered", "QuantityReceived", "QuantityCancelled", "CreatedAt")
    VALUES
        (gen_random_uuid(), v_tenant_id, v_order_id, 1, v_product_ids[7], v_product_skus[7], 150, 150, 0, NOW() - INTERVAL '10 days'),
        (gen_random_uuid(), v_tenant_id, v_order_id, 2, v_product_ids[8], v_product_skus[8], 150, 0, 0, NOW() - INTERVAL '10 days');

    -- Order 5: Fully Received
    v_order_id := gen_random_uuid();
    INSERT INTO "PurchaseOrders" ("Id", "TenantId", "OrderNumber", "SupplierId", "WarehouseId", "Status",
        "OrderDate", "ExpectedDate", "ReceivedDate", "TotalLines", "TotalQuantity", "ReceivedQuantity", "Notes", "CreatedAt")
    VALUES (v_order_id, v_tenant_id, 'PO-2024-0005', v_supplier_ids[5], v_warehouse_id, 4,
        NOW() - INTERVAL '15 days', NOW() - INTERVAL '5 days', NOW() - INTERVAL '6 days', 3, 500, 500, 'Received in full - quality check passed', NOW() - INTERVAL '15 days');
    INSERT INTO "PurchaseOrderLines" ("Id", "TenantId", "OrderId", "LineNumber", "ProductId", "Sku", "QuantityOrdered", "QuantityReceived", "QuantityCancelled", "CreatedAt")
    VALUES
        (gen_random_uuid(), v_tenant_id, v_order_id, 1, v_product_ids[1], v_product_skus[1], 200, 200, 0, NOW() - INTERVAL '15 days'),
        (gen_random_uuid(), v_tenant_id, v_order_id, 2, v_product_ids[2], v_product_skus[2], 150, 150, 0, NOW() - INTERVAL '15 days'),
        (gen_random_uuid(), v_tenant_id, v_order_id, 3, v_product_ids[3], v_product_skus[3], 150, 150, 0, NOW() - INTERVAL '15 days');

    -- Order 6: Closed
    v_order_id := gen_random_uuid();
    INSERT INTO "PurchaseOrders" ("Id", "TenantId", "OrderNumber", "SupplierId", "WarehouseId", "Status",
        "OrderDate", "ExpectedDate", "ReceivedDate", "TotalLines", "TotalQuantity", "ReceivedQuantity", "Notes", "CreatedAt")
    VALUES (v_order_id, v_tenant_id, 'PO-2024-0006', v_supplier_ids[6], v_warehouse_id, 5,
        NOW() - INTERVAL '30 days', NOW() - INTERVAL '16 days', NOW() - INTERVAL '17 days', 2, 400, 400, 'Completed and closed', NOW() - INTERVAL '30 days');
    INSERT INTO "PurchaseOrderLines" ("Id", "TenantId", "OrderId", "LineNumber", "ProductId", "Sku", "QuantityOrdered", "QuantityReceived", "QuantityCancelled", "CreatedAt")
    VALUES
        (gen_random_uuid(), v_tenant_id, v_order_id, 1, v_product_ids[4], v_product_skus[4], 200, 200, 0, NOW() - INTERVAL '30 days'),
        (gen_random_uuid(), v_tenant_id, v_order_id, 2, v_product_ids[5], v_product_skus[5], 200, 200, 0, NOW() - INTERVAL '30 days');

    -- Order 7: Cancelled
    v_order_id := gen_random_uuid();
    INSERT INTO "PurchaseOrders" ("Id", "TenantId", "OrderNumber", "SupplierId", "WarehouseId", "Status",
        "OrderDate", "ExpectedDate", "TotalLines", "TotalQuantity", "ReceivedQuantity", "Notes", "CreatedAt")
    VALUES (v_order_id, v_tenant_id, 'PO-2024-0007', v_supplier_ids[7], v_warehouse_id, 6,
        NOW() - INTERVAL '20 days', NOW() - INTERVAL '6 days', 1, 100, 0, 'Cancelled - supplier unable to fulfill', NOW() - INTERVAL '20 days');
    INSERT INTO "PurchaseOrderLines" ("Id", "TenantId", "OrderId", "LineNumber", "ProductId", "Sku", "QuantityOrdered", "QuantityReceived", "QuantityCancelled", "Notes", "CreatedAt")
    VALUES (gen_random_uuid(), v_tenant_id, v_order_id, 1, v_product_ids[9], v_product_skus[9], 100, 0, 100, 'Item discontinued by supplier', NOW() - INTERVAL '20 days');

    -- Order 8: Confirmed - urgent
    v_order_id := gen_random_uuid();
    INSERT INTO "PurchaseOrders" ("Id", "TenantId", "OrderNumber", "ExternalReference", "SupplierId", "WarehouseId", "Status",
        "OrderDate", "ExpectedDate", "TotalLines", "TotalQuantity", "ReceivedQuantity", "Notes", "InternalNotes", "CreatedAt")
    VALUES (v_order_id, v_tenant_id, 'PO-2024-0008', 'URGENT-RESTOCK-001', v_supplier_ids[1], v_warehouse_id, 2,
        NOW() - INTERVAL '1 day', NOW() + INTERVAL '2 days', 2, 80, 0, 'URGENT - Stock replenishment', 'Expedited shipping confirmed', NOW() - INTERVAL '1 day');
    INSERT INTO "PurchaseOrderLines" ("Id", "TenantId", "OrderId", "LineNumber", "ProductId", "Sku", "QuantityOrdered", "QuantityReceived", "QuantityCancelled", "CreatedAt")
    VALUES
        (gen_random_uuid(), v_tenant_id, v_order_id, 1, v_product_ids[10], v_product_skus[10], 50, 0, 0, NOW() - INTERVAL '1 day'),
        (gen_random_uuid(), v_tenant_id, v_order_id, 2, v_product_ids[6], v_product_skus[6], 30, 0, 0, NOW() - INTERVAL '1 day');

    -- Order 9: Confirmed - with batch/expiry tracking
    v_order_id := gen_random_uuid();
    INSERT INTO "PurchaseOrders" ("Id", "TenantId", "OrderNumber", "SupplierId", "WarehouseId", "Status",
        "OrderDate", "ExpectedDate", "TotalLines", "TotalQuantity", "ReceivedQuantity", "Notes", "CreatedAt")
    VALUES (v_order_id, v_tenant_id, 'PO-2024-0009', v_supplier_ids[2], v_warehouse_id, 2,
        NOW() - INTERVAL '3 days', NOW() + INTERVAL '7 days', 2, 120, 0, 'Perishable items - batch tracking required', NOW() - INTERVAL '3 days');
    INSERT INTO "PurchaseOrderLines" ("Id", "TenantId", "OrderId", "LineNumber", "ProductId", "Sku", "QuantityOrdered", "QuantityReceived", "QuantityCancelled", "ExpectedBatchNumber", "ExpectedExpiryDate", "CreatedAt")
    VALUES
        (gen_random_uuid(), v_tenant_id, v_order_id, 1, v_product_ids[7], v_product_skus[7], 60, 0, 0, 'BATCH-2024-FEB', NOW() + INTERVAL '180 days', NOW() - INTERVAL '3 days'),
        (gen_random_uuid(), v_tenant_id, v_order_id, 2, v_product_ids[8], v_product_skus[8], 60, 0, 0, 'BATCH-2024-FEB', NOW() + INTERVAL '180 days', NOW() - INTERVAL '3 days');

    -- Order 10: Pending - large order
    v_order_id := gen_random_uuid();
    INSERT INTO "PurchaseOrders" ("Id", "TenantId", "OrderNumber", "SupplierId", "WarehouseId", "Status",
        "OrderDate", "ExpectedDate", "TotalLines", "TotalQuantity", "ReceivedQuantity", "Notes", "CreatedAt")
    VALUES (v_order_id, v_tenant_id, 'PO-2024-0010', v_supplier_ids[4], v_warehouse_id, 1,
        NOW() - INTERVAL '1 day', NOW() + INTERVAL '28 days', 5, 1000, 0, 'Bulk quarterly order', NOW() - INTERVAL '1 day');
    INSERT INTO "PurchaseOrderLines" ("Id", "TenantId", "OrderId", "LineNumber", "ProductId", "Sku", "QuantityOrdered", "QuantityReceived", "QuantityCancelled", "CreatedAt")
    VALUES
        (gen_random_uuid(), v_tenant_id, v_order_id, 1, v_product_ids[1], v_product_skus[1], 200, 0, 0, NOW() - INTERVAL '1 day'),
        (gen_random_uuid(), v_tenant_id, v_order_id, 2, v_product_ids[2], v_product_skus[2], 200, 0, 0, NOW() - INTERVAL '1 day'),
        (gen_random_uuid(), v_tenant_id, v_order_id, 3, v_product_ids[3], v_product_skus[3], 200, 0, 0, NOW() - INTERVAL '1 day'),
        (gen_random_uuid(), v_tenant_id, v_order_id, 4, v_product_ids[4], v_product_skus[4], 200, 0, 0, NOW() - INTERVAL '1 day'),
        (gen_random_uuid(), v_tenant_id, v_order_id, 5, v_product_ids[5], v_product_skus[5], 200, 0, 0, NOW() - INTERVAL '1 day');

    RAISE NOTICE 'Successfully inserted 10 purchase orders with lines';
END $$;

-- Status enum values reference:
-- 0 = Draft, 1 = Pending, 2 = Confirmed, 3 = PartiallyReceived
-- 4 = Received, 5 = Closed, 6 = Cancelled

-- Useful queries:
-- SELECT * FROM "PurchaseOrders" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
-- SELECT po.*, pol.* FROM "PurchaseOrders" po JOIN "PurchaseOrderLines" pol ON po."Id" = pol."OrderId" WHERE po."TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
