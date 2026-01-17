-- ============================================
-- SmartWMS - Seed Goods Receipts Data
-- PostgreSQL
-- ============================================
-- TenantId: e9006ab8-257f-4021-b60a-cbba785bad46
-- WarehouseId: ae5e6339-6212-4435-89a9-f5940815fe20
-- ============================================
-- GoodsReceiptStatus enum: 0=Draft, 1=InProgress, 2=Complete, 3=PartiallyComplete, 4=Cancelled
-- GoodsReceiptLineStatus enum: 0=Pending, 1=Received, 2=PartiallyReceived, 3=Rejected, 4=PutAway
-- ============================================

DO $$
DECLARE
    v_tenant_id UUID := 'e9006ab8-257f-4021-b60a-cbba785bad46';
    v_warehouse_id UUID := 'ae5e6339-6212-4435-89a9-f5940815fe20';
    v_supplier_ids UUID[];
    v_product_ids UUID[];
    v_product_skus TEXT[];
    v_rcv_location_id UUID;
    v_receipt_id UUID;
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "GoodsReceipts" WHERE "TenantId" = v_tenant_id LIMIT 1) THEN

        -- Get receiving location
        SELECT "Id" INTO v_rcv_location_id
        FROM "Locations"
        WHERE "TenantId" = v_tenant_id AND "Code" = 'DOCK-RCV-01'
        LIMIT 1;

        -- Get suppliers
        SELECT ARRAY_AGG("Id" ORDER BY "Code")
        INTO v_supplier_ids
        FROM (SELECT "Id", "Code" FROM "Suppliers"
              WHERE "TenantId" = v_tenant_id AND "IsActive" = true
              ORDER BY "Code" LIMIT 5) s;

        -- Get products
        SELECT ARRAY_AGG("Id" ORDER BY "Sku"), ARRAY_AGG("Sku" ORDER BY "Sku")
        INTO v_product_ids, v_product_skus
        FROM (SELECT "Id", "Sku" FROM "Products"
              WHERE "TenantId" = v_tenant_id AND "IsActive" = true
              ORDER BY "Sku" LIMIT 15) p;

        IF v_supplier_ids IS NULL OR v_product_ids IS NULL THEN
            RAISE NOTICE 'Missing suppliers or products';
            RETURN;
        END IF;

        -- GR 1: Complete receipt
        v_receipt_id := gen_random_uuid();
        INSERT INTO "GoodsReceipts" (
            "Id", "TenantId", "ReceiptNumber", "PurchaseOrderId", "SupplierId",
            "WarehouseId", "ReceivingLocationId", "Status",
            "ReceiptDate", "CompletedAt",
            "CarrierName", "TrackingNumber", "DeliveryNote",
            "TotalLines", "TotalQuantityExpected", "TotalQuantityReceived",
            "Notes", "CreatedAt"
        ) VALUES (
            v_receipt_id, v_tenant_id, 'GR-2024-0001', NULL, v_supplier_ids[1],
            v_warehouse_id, v_rcv_location_id, 2, -- Complete
            NOW() - INTERVAL '10 days', NOW() - INTERVAL '10 days',
            'DHL', 'JD014600006753906201', 'DN-2024-12345',
            3, 150, 150,
            'Full shipment received', NOW()
        );

        INSERT INTO "GoodsReceiptLines" (
            "Id", "TenantId", "ReceiptId", "LineNumber",
            "ProductId", "Sku",
            "QuantityExpected", "QuantityReceived", "QuantityRejected",
            "BatchNumber", "ExpirationDate",
            "Status", "QualityStatus", "Notes", "CreatedAt"
        ) VALUES
        (gen_random_uuid(), v_tenant_id, v_receipt_id, 1, v_product_ids[1], v_product_skus[1], 50, 50, 0, 'BATCH-2024-001', NOW() + INTERVAL '180 days', 4, 'Good', NULL, NOW()),
        (gen_random_uuid(), v_tenant_id, v_receipt_id, 2, v_product_ids[2], v_product_skus[2], 50, 50, 0, 'BATCH-2024-002', NOW() + INTERVAL '180 days', 4, 'Good', NULL, NOW()),
        (gen_random_uuid(), v_tenant_id, v_receipt_id, 3, v_product_ids[3], v_product_skus[3], 50, 50, 0, 'BATCH-2024-003', NOW() + INTERVAL '180 days', 4, 'Good', NULL, NOW());

        -- GR 2: In Progress receipt
        v_receipt_id := gen_random_uuid();
        INSERT INTO "GoodsReceipts" (
            "Id", "TenantId", "ReceiptNumber", "PurchaseOrderId", "SupplierId",
            "WarehouseId", "ReceivingLocationId", "Status",
            "ReceiptDate",
            "CarrierName", "TrackingNumber",
            "TotalLines", "TotalQuantityExpected", "TotalQuantityReceived",
            "Notes", "CreatedAt"
        ) VALUES (
            v_receipt_id, v_tenant_id, 'GR-2024-0002', NULL, v_supplier_ids[2],
            v_warehouse_id, v_rcv_location_id, 1, -- InProgress
            NOW() - INTERVAL '1 day',
            'PostNord', 'SE1234567890',
            2, 100, 60,
            'Partial receipt - awaiting remaining items', NOW()
        );

        INSERT INTO "GoodsReceiptLines" (
            "Id", "TenantId", "ReceiptId", "LineNumber",
            "ProductId", "Sku",
            "QuantityExpected", "QuantityReceived", "QuantityRejected",
            "BatchNumber", "ExpirationDate",
            "Status", "QualityStatus", "Notes", "CreatedAt"
        ) VALUES
        (gen_random_uuid(), v_tenant_id, v_receipt_id, 1, v_product_ids[4], v_product_skus[4], 60, 60, 0, 'BATCH-2024-004', NOW() + INTERVAL '365 days', 1, 'Good', 'Fully received', NOW()),
        (gen_random_uuid(), v_tenant_id, v_receipt_id, 2, v_product_ids[5], v_product_skus[5], 40, 0, 0, NULL, NULL, 0, NULL, 'Pending arrival', NOW());

        -- GR 3: Partially Complete with rejection
        v_receipt_id := gen_random_uuid();
        INSERT INTO "GoodsReceipts" (
            "Id", "TenantId", "ReceiptNumber", "PurchaseOrderId", "SupplierId",
            "WarehouseId", "ReceivingLocationId", "Status",
            "ReceiptDate", "CompletedAt",
            "CarrierName", "TrackingNumber",
            "TotalLines", "TotalQuantityExpected", "TotalQuantityReceived",
            "Notes", "InternalNotes", "CreatedAt"
        ) VALUES (
            v_receipt_id, v_tenant_id, 'GR-2024-0003', NULL, v_supplier_ids[3],
            v_warehouse_id, v_rcv_location_id, 3, -- PartiallyComplete
            NOW() - INTERVAL '5 days', NOW() - INTERVAL '5 days',
            'DB Schenker', 'SCH123456789',
            3, 200, 175,
            'Some items damaged during transit', 'Claim filed with carrier', NOW()
        );

        INSERT INTO "GoodsReceiptLines" (
            "Id", "TenantId", "ReceiptId", "LineNumber",
            "ProductId", "Sku",
            "QuantityExpected", "QuantityReceived", "QuantityRejected",
            "BatchNumber",
            "Status", "QualityStatus", "RejectionReason", "Notes", "CreatedAt"
        ) VALUES
        (gen_random_uuid(), v_tenant_id, v_receipt_id, 1, v_product_ids[6], v_product_skus[6], 80, 80, 0, 'BATCH-2024-006', 4, 'Good', NULL, NULL, NOW()),
        (gen_random_uuid(), v_tenant_id, v_receipt_id, 2, v_product_ids[7], v_product_skus[7], 80, 55, 25, 'BATCH-2024-007', 1, 'Damaged', 'Packaging damaged - water damage', '25 units rejected', NOW()),
        (gen_random_uuid(), v_tenant_id, v_receipt_id, 3, v_product_ids[8], v_product_skus[8], 40, 40, 0, 'BATCH-2024-008', 4, 'Good', NULL, NULL, NOW());

        -- GR 4: Draft receipt (not started)
        v_receipt_id := gen_random_uuid();
        INSERT INTO "GoodsReceipts" (
            "Id", "TenantId", "ReceiptNumber", "PurchaseOrderId", "SupplierId",
            "WarehouseId", "ReceivingLocationId", "Status",
            "ReceiptDate",
            "CarrierName",
            "TotalLines", "TotalQuantityExpected", "TotalQuantityReceived",
            "Notes", "CreatedAt"
        ) VALUES (
            v_receipt_id, v_tenant_id, 'GR-2024-0004', NULL, v_supplier_ids[4],
            v_warehouse_id, v_rcv_location_id, 0, -- Draft
            NOW() + INTERVAL '2 days',
            'FedEx',
            2, 120, 0,
            'Scheduled arrival Thursday', NOW()
        );

        INSERT INTO "GoodsReceiptLines" (
            "Id", "TenantId", "ReceiptId", "LineNumber",
            "ProductId", "Sku",
            "QuantityExpected", "QuantityReceived", "QuantityRejected",
            "Status", "CreatedAt"
        ) VALUES
        (gen_random_uuid(), v_tenant_id, v_receipt_id, 1, v_product_ids[9], v_product_skus[9], 70, 0, 0, 0, NOW()),
        (gen_random_uuid(), v_tenant_id, v_receipt_id, 2, v_product_ids[10], v_product_skus[10], 50, 0, 0, 0, NOW());

        -- GR 5: Cancelled receipt
        v_receipt_id := gen_random_uuid();
        INSERT INTO "GoodsReceipts" (
            "Id", "TenantId", "ReceiptNumber", "PurchaseOrderId", "SupplierId",
            "WarehouseId", "ReceivingLocationId", "Status",
            "ReceiptDate",
            "TotalLines", "TotalQuantityExpected", "TotalQuantityReceived",
            "Notes", "CreatedAt"
        ) VALUES (
            v_receipt_id, v_tenant_id, 'GR-2024-0005', NULL, v_supplier_ids[1],
            v_warehouse_id, v_rcv_location_id, 4, -- Cancelled
            NOW() - INTERVAL '15 days',
            1, 50, 0,
            'Order cancelled by supplier', NOW()
        );

        INSERT INTO "GoodsReceiptLines" (
            "Id", "TenantId", "ReceiptId", "LineNumber",
            "ProductId", "Sku",
            "QuantityExpected", "QuantityReceived", "QuantityRejected",
            "Status", "Notes", "CreatedAt"
        ) VALUES
        (gen_random_uuid(), v_tenant_id, v_receipt_id, 1, v_product_ids[11], v_product_skus[11], 50, 0, 0, 0, 'Supplier out of stock', NOW());

        RAISE NOTICE 'Successfully inserted 5 goods receipts with lines';
    ELSE
        RAISE NOTICE 'Goods receipts already exist, skipping seed';
    END IF;
END $$;

-- GoodsReceiptStatus enum reference:
-- 0 = Draft, 1 = InProgress, 2 = Complete, 3 = PartiallyComplete, 4 = Cancelled

-- GoodsReceiptLineStatus enum reference:
-- 0 = Pending, 1 = Received, 2 = PartiallyReceived, 3 = Rejected, 4 = PutAway

-- Useful queries:
-- SELECT * FROM "GoodsReceipts" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
-- SELECT gr.*, grl.* FROM "GoodsReceipts" gr JOIN "GoodsReceiptLines" grl ON gr."Id" = grl."ReceiptId" WHERE gr."TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
