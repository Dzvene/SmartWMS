-- ============================================
-- SmartWMS - Seed Stock Adjustments & Transfers Data
-- PostgreSQL
-- ============================================
-- TenantId: e9006ab8-257f-4021-b60a-cbba785bad46
-- WarehouseId: ae5e6339-6212-4435-89a9-f5940815fe20
-- ============================================
-- AdjustmentStatus enum: 0=Draft, 1=PendingApproval, 2=Approved, 3=Posted, 4=Cancelled
-- AdjustmentType enum: 0=Correction, 1=CycleCount, 2=Damage, 3=Scrap, 4=Found, 5=Lost, 6=Expiry, 7=QualityHold, 8=Revaluation, 9=Opening, 10=Other
-- TransferType enum: 0=Internal, 1=InterWarehouse, 2=Replenishment, 3=Return, 4=Consolidation, 5=Relocation
-- TransferStatus enum: 0=Draft, 1=Requested, 2=Approved, 3=Released, 4=InProgress, 5=Picked, 6=InTransit, 7=Received, 8=Complete, 9=Cancelled
-- TransferPriority enum: 0=Low, 1=Normal, 2=High, 3=Urgent
-- ============================================

DO $$
DECLARE
    v_tenant_id UUID := 'e9006ab8-257f-4021-b60a-cbba785bad46';
    v_warehouse_id UUID := 'ae5e6339-6212-4435-89a9-f5940815fe20';
    v_wh_gothenburg UUID := 'cf708551-8434-6657-9bc1-b7162037ba42';
    v_zone_pck_a UUID := 'a5555555-5555-5555-5555-555555555555';
    v_zone_blk_a UUID := 'a3333333-3333-3333-3333-333333333333';
    v_admin_user_id UUID;
    v_location_ids UUID[];
    v_bulk_location_ids UUID[];
    v_product_ids UUID[];
    v_product_skus TEXT[];
    v_adj_id UUID;
    v_transfer_id UUID;
BEGIN
    -- Get admin user for CreatedByUserId
    SELECT "Id" INTO v_admin_user_id FROM "Users" WHERE "TenantId" = v_tenant_id LIMIT 1;
    -- Get pick locations
    SELECT ARRAY_AGG("Id" ORDER BY "Code")
    INTO v_location_ids
    FROM (SELECT "Id", "Code" FROM "Locations"
          WHERE "TenantId" = v_tenant_id AND "IsPickLocation" = true
          ORDER BY "Code" LIMIT 10) l;

    -- Get bulk locations
    SELECT ARRAY_AGG("Id" ORDER BY "Code")
    INTO v_bulk_location_ids
    FROM (SELECT "Id", "Code" FROM "Locations"
          WHERE "TenantId" = v_tenant_id AND "Code" LIKE 'A-01%' AND "IsPutawayLocation" = true
          ORDER BY "Code" LIMIT 10) bl;

    -- Get products
    SELECT ARRAY_AGG("Id" ORDER BY "Sku"), ARRAY_AGG("Sku" ORDER BY "Sku")
    INTO v_product_ids, v_product_skus
    FROM (SELECT "Id", "Sku" FROM "Products"
          WHERE "TenantId" = v_tenant_id AND "IsActive" = true
          ORDER BY "Sku" LIMIT 10) p;

    IF v_location_ids IS NULL OR v_product_ids IS NULL THEN
        RAISE NOTICE 'Missing locations or products';
        RETURN;
    END IF;

    -- ==========================================
    -- STOCK ADJUSTMENTS
    -- ==========================================
    IF NOT EXISTS (SELECT 1 FROM "StockAdjustments" WHERE "TenantId" = v_tenant_id LIMIT 1) THEN

        -- Adjustment 1: Posted (completed)
        v_adj_id := gen_random_uuid();
        INSERT INTO "StockAdjustments" (
            "Id", "TenantId", "AdjustmentNumber", "WarehouseId",
            "Status", "AdjustmentType",
            "ReasonNotes", "SourceDocumentType", "SourceDocumentNumber",
            "CreatedByUserId", "ApprovedByUserId", "ApprovedAt",
            "PostedByUserId", "PostedAt",
            "TotalLines", "TotalQuantityChange", "TotalValueChange",
            "Notes", "CreatedAt"
        ) VALUES (
            v_adj_id, v_tenant_id, 'ADJ-2024-0001', v_warehouse_id,
            3, 1, -- Posted, CycleCount
            'Variance from weekly count', 'CycleCount', 'CC-2024-0001',
            v_admin_user_id, v_admin_user_id, NOW() - INTERVAL '5 days',
            v_admin_user_id, NOW() - INTERVAL '5 days',
            2, -7, -350.00,
            'Cycle count adjustment processed', NOW()
        );

        INSERT INTO "StockAdjustmentLines" (
            "Id", "TenantId", "AdjustmentId", "LineNumber",
            "ProductId", "Sku", "LocationId",
            "QuantityBefore", "QuantityAdjustment",
            "UnitCost",
            "ReasonNotes", "IsProcessed", "ProcessedAt", "CreatedAt"
        ) VALUES
        (gen_random_uuid(), v_tenant_id, v_adj_id, 1, v_product_ids[1], v_product_skus[1], v_location_ids[1],
         100, -5, 50.00, 'Count variance', true, NOW() - INTERVAL '5 days', NOW()),
        (gen_random_uuid(), v_tenant_id, v_adj_id, 2, v_product_ids[2], v_product_skus[2], v_location_ids[2],
         50, -2, 50.00, 'Count variance', true, NOW() - INTERVAL '5 days', NOW());

        -- Adjustment 2: Pending Approval
        v_adj_id := gen_random_uuid();
        INSERT INTO "StockAdjustments" (
            "Id", "TenantId", "AdjustmentNumber", "WarehouseId",
            "Status", "AdjustmentType",
            "ReasonNotes",
            "CreatedByUserId",
            "TotalLines", "TotalQuantityChange", "TotalValueChange",
            "Notes", "CreatedAt"
        ) VALUES (
            v_adj_id, v_tenant_id, 'ADJ-2024-0002', v_warehouse_id,
            1, 2, -- PendingApproval, Damage
            'Forklift accident - damaged pallet',
            v_admin_user_id,
            1, -15, -750.00,
            'Awaiting supervisor approval', NOW()
        );

        INSERT INTO "StockAdjustmentLines" (
            "Id", "TenantId", "AdjustmentId", "LineNumber",
            "ProductId", "Sku", "LocationId",
            "QuantityBefore", "QuantityAdjustment",
            "UnitCost",
            "ReasonNotes", "IsProcessed", "CreatedAt"
        ) VALUES
        (gen_random_uuid(), v_tenant_id, v_adj_id, 1, v_product_ids[3], v_product_skus[3], v_bulk_location_ids[1],
         200, -15, 50.00, 'Forklift damage - cartons crushed', false, NOW());

        -- Adjustment 3: Approved (not yet posted)
        v_adj_id := gen_random_uuid();
        INSERT INTO "StockAdjustments" (
            "Id", "TenantId", "AdjustmentNumber", "WarehouseId",
            "Status", "AdjustmentType",
            "ReasonNotes",
            "CreatedByUserId", "ApprovedByUserId", "ApprovedAt",
            "TotalLines", "TotalQuantityChange",
            "Notes", "CreatedAt"
        ) VALUES (
            v_adj_id, v_tenant_id, 'ADJ-2024-0003', v_warehouse_id,
            2, 4, -- Approved, Found
            'Extra items found during inventory cleanup',
            v_admin_user_id, v_admin_user_id, NOW() - INTERVAL '1 hour',
            1, 8,
            'Ready for posting', NOW()
        );

        INSERT INTO "StockAdjustmentLines" (
            "Id", "TenantId", "AdjustmentId", "LineNumber",
            "ProductId", "Sku", "LocationId",
            "QuantityBefore", "QuantityAdjustment",
            "ReasonNotes", "IsProcessed", "CreatedAt"
        ) VALUES
        (gen_random_uuid(), v_tenant_id, v_adj_id, 1, v_product_ids[4], v_product_skus[4], v_location_ids[4],
         30, 8, 'Found behind shelving unit', false, NOW());

        -- Adjustment 4: Draft
        v_adj_id := gen_random_uuid();
        INSERT INTO "StockAdjustments" (
            "Id", "TenantId", "AdjustmentNumber", "WarehouseId",
            "Status", "AdjustmentType",
            "ReasonNotes", "CreatedByUserId",
            "TotalLines", "TotalQuantityChange",
            "Notes", "CreatedAt"
        ) VALUES (
            v_adj_id, v_tenant_id, 'ADJ-2024-0004', v_warehouse_id,
            0, 6, -- Draft, Expiry
            'Expired products - batch check',
            v_admin_user_id,
            2, -20,
            'Draft - verifying quantities', NOW()
        );

        INSERT INTO "StockAdjustmentLines" (
            "Id", "TenantId", "AdjustmentId", "LineNumber",
            "ProductId", "Sku", "LocationId", "BatchNumber",
            "QuantityBefore", "QuantityAdjustment",
            "ReasonNotes", "IsProcessed", "CreatedAt"
        ) VALUES
        (gen_random_uuid(), v_tenant_id, v_adj_id, 1, v_product_ids[5], v_product_skus[5], v_location_ids[5], 'BATCH-EXP-001',
         25, -12, 'Expired 2024-01-01', false, NOW()),
        (gen_random_uuid(), v_tenant_id, v_adj_id, 2, v_product_ids[6], v_product_skus[6], v_location_ids[6], 'BATCH-EXP-002',
         18, -8, 'Expired 2024-01-05', false, NOW());

        RAISE NOTICE 'Successfully inserted 4 stock adjustments with lines';
    END IF;

    -- ==========================================
    -- STOCK TRANSFERS
    -- ==========================================
    IF NOT EXISTS (SELECT 1 FROM "StockTransfers" WHERE "TenantId" = v_tenant_id LIMIT 1) THEN

        -- Transfer 1: Complete (replenishment)
        v_transfer_id := gen_random_uuid();
        INSERT INTO "StockTransfers" (
            "Id", "TenantId", "TransferNumber",
            "TransferType", "FromWarehouseId", "FromZoneId",
            "ToWarehouseId", "ToZoneId",
            "Status", "Priority",
            "ReasonNotes",
            "CreatedByUserId", "PickedByUserId", "PickedAt",
            "ReceivedByUserId", "ReceivedAt",
            "TotalLines", "TotalQuantity", "PickedLines", "ReceivedLines",
            "Notes", "CreatedAt"
        ) VALUES (
            v_transfer_id, v_tenant_id, 'TRF-2024-0001',
            2, v_warehouse_id, v_zone_blk_a, -- Replenishment
            v_warehouse_id, v_zone_pck_a,
            8, 1, -- Complete, Normal
            'Pick location replenishment',
            v_admin_user_id, v_admin_user_id, NOW() - INTERVAL '3 days',
            v_admin_user_id, NOW() - INTERVAL '3 days',
            2, 100, 2, 2,
            'Routine replenishment completed', NOW()
        );

        INSERT INTO "StockTransferLines" (
            "Id", "TenantId", "TransferId", "LineNumber",
            "ProductId", "Sku", "FromLocationId", "ToLocationId",
            "QuantityRequested", "QuantityPicked", "QuantityReceived",
            "Status", "PickedAt", "ReceivedAt", "CreatedAt"
        ) VALUES
        (gen_random_uuid(), v_tenant_id, v_transfer_id, 1, v_product_ids[1], v_product_skus[1], v_bulk_location_ids[1], v_location_ids[1],
         50, 50, 50, 7, NOW() - INTERVAL '3 days', NOW() - INTERVAL '3 days', NOW()), -- Received
        (gen_random_uuid(), v_tenant_id, v_transfer_id, 2, v_product_ids[2], v_product_skus[2], v_bulk_location_ids[2], v_location_ids[2],
         50, 50, 50, 7, NOW() - INTERVAL '3 days', NOW() - INTERVAL '3 days', NOW()); -- Received

        -- Transfer 2: In Progress
        v_transfer_id := gen_random_uuid();
        INSERT INTO "StockTransfers" (
            "Id", "TenantId", "TransferNumber",
            "TransferType", "FromWarehouseId", "FromZoneId",
            "ToWarehouseId", "ToZoneId",
            "Status", "Priority",
            "ReasonNotes", "CreatedByUserId", "AssignedToUserId",
            "TotalLines", "TotalQuantity", "PickedLines", "ReceivedLines",
            "Notes", "CreatedAt"
        ) VALUES (
            v_transfer_id, v_tenant_id, 'TRF-2024-0002',
            0, v_warehouse_id, v_zone_blk_a, -- Internal
            v_warehouse_id, v_zone_pck_a,
            4, 2, -- InProgress, High
            'Urgent replenishment for high-demand SKU',
            v_admin_user_id, v_admin_user_id,
            2, 80, 1, 0,
            'Picking in progress', NOW()
        );

        INSERT INTO "StockTransferLines" (
            "Id", "TenantId", "TransferId", "LineNumber",
            "ProductId", "Sku", "FromLocationId", "ToLocationId",
            "QuantityRequested", "QuantityPicked", "QuantityReceived",
            "Status", "PickedAt", "CreatedAt"
        ) VALUES
        (gen_random_uuid(), v_tenant_id, v_transfer_id, 1, v_product_ids[3], v_product_skus[3], v_bulk_location_ids[3], v_location_ids[3],
         40, 40, 0, 4, NOW() - INTERVAL '30 minutes', NOW()), -- Picked
        (gen_random_uuid(), v_tenant_id, v_transfer_id, 2, v_product_ids[4], v_product_skus[4], v_bulk_location_ids[4], v_location_ids[4],
         40, 0, 0, 0, NULL, NOW()); -- Pending

        -- Transfer 3: Released (ready to pick)
        v_transfer_id := gen_random_uuid();
        INSERT INTO "StockTransfers" (
            "Id", "TenantId", "TransferNumber",
            "TransferType", "FromWarehouseId", "ToWarehouseId",
            "Status", "Priority",
            "ScheduledDate", "RequiredByDate",
            "ReasonNotes", "CreatedByUserId",
            "TotalLines", "TotalQuantity", "PickedLines", "ReceivedLines",
            "Notes", "CreatedAt"
        ) VALUES (
            v_transfer_id, v_tenant_id, 'TRF-2024-0003',
            5, v_warehouse_id, v_warehouse_id, -- Relocation
            3, 1, -- Released, Normal
            NOW(), NOW() + INTERVAL '1 day',
            'Warehouse reorganization',
            v_admin_user_id,
            3, 150, 0, 0,
            'Ready for picking', NOW()
        );

        INSERT INTO "StockTransferLines" (
            "Id", "TenantId", "TransferId", "LineNumber",
            "ProductId", "Sku", "FromLocationId", "ToLocationId",
            "QuantityRequested", "QuantityPicked", "QuantityReceived",
            "Status", "CreatedAt"
        ) VALUES
        (gen_random_uuid(), v_tenant_id, v_transfer_id, 1, v_product_ids[5], v_product_skus[5], v_location_ids[5], v_bulk_location_ids[5],
         50, 0, 0, 0, NOW()),
        (gen_random_uuid(), v_tenant_id, v_transfer_id, 2, v_product_ids[6], v_product_skus[6], v_location_ids[6], v_bulk_location_ids[6],
         50, 0, 0, 0, NOW()),
        (gen_random_uuid(), v_tenant_id, v_transfer_id, 3, v_product_ids[7], v_product_skus[7], v_location_ids[7], v_bulk_location_ids[7],
         50, 0, 0, 0, NOW());

        -- Transfer 4: Draft
        INSERT INTO "StockTransfers" (
            "Id", "TenantId", "TransferNumber",
            "TransferType", "FromWarehouseId", "ToWarehouseId",
            "Status", "Priority",
            "ReasonNotes", "CreatedByUserId",
            "TotalLines", "TotalQuantity", "PickedLines", "ReceivedLines",
            "Notes", "CreatedAt"
        ) VALUES (
            gen_random_uuid(), v_tenant_id, 'TRF-2024-0004',
            4, v_warehouse_id, v_warehouse_id, -- Consolidation
            0, 1, -- Draft, Normal
            'Consolidating slow-moving inventory',
            v_admin_user_id,
            0, 0, 0, 0,
            'Planning in progress', NOW()
        );

        -- Transfer 5: Urgent internal transfer (requested)
        v_transfer_id := gen_random_uuid();
        INSERT INTO "StockTransfers" (
            "Id", "TenantId", "TransferNumber",
            "TransferType", "FromWarehouseId", "ToWarehouseId",
            "Status", "Priority",
            "ScheduledDate",
            "ReasonNotes", "CreatedByUserId",
            "TotalLines", "TotalQuantity", "PickedLines", "ReceivedLines",
            "Notes", "CreatedAt"
        ) VALUES (
            v_transfer_id, v_tenant_id, 'TRF-2024-0005',
            0, v_warehouse_id, v_warehouse_id, -- Internal
            1, 3, -- Requested, Urgent
            NOW() + INTERVAL '2 days',
            'Urgent stock movement - high demand area',
            v_admin_user_id,
            2, 100, 0, 0,
            'Awaiting approval for urgent transfer', NOW()
        );

        INSERT INTO "StockTransferLines" (
            "Id", "TenantId", "TransferId", "LineNumber",
            "ProductId", "Sku", "FromLocationId", "ToLocationId",
            "QuantityRequested", "QuantityPicked", "QuantityReceived",
            "Status", "CreatedAt"
        ) VALUES
        (gen_random_uuid(), v_tenant_id, v_transfer_id, 1, v_product_ids[8], v_product_skus[8], v_bulk_location_ids[8], v_location_ids[8],
         60, 0, 0, 0, NOW()),
        (gen_random_uuid(), v_tenant_id, v_transfer_id, 2, v_product_ids[9], v_product_skus[9], v_bulk_location_ids[9], v_location_ids[9],
         40, 0, 0, 0, NOW());

        RAISE NOTICE 'Successfully inserted 5 stock transfers with lines';
    END IF;
END $$;

-- Useful queries:
-- SELECT * FROM "StockAdjustments" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
-- SELECT * FROM "StockTransfers" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
