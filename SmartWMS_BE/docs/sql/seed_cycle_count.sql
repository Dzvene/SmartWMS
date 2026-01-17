-- ============================================
-- SmartWMS - Seed Cycle Count Data
-- PostgreSQL
-- ============================================
-- TenantId: e9006ab8-257f-4021-b60a-cbba785bad46
-- WarehouseId: ae5e6339-6212-4435-89a9-f5940815fe20
-- ============================================
-- CountType enum: 0=Scheduled, 1=Random, 2=ABC, 3=Perpetual, 4=Annual, 5=Triggered
-- CountScope enum: 0=Location, 1=Product, 2=Zone, 3=Warehouse
-- CycleCountStatus enum: 0=Draft, 1=Scheduled, 2=InProgress, 3=Review, 4=Complete, 5=Cancelled
-- CountItemStatus enum: 0=Pending, 1=Counted, 2=Recounting, 3=Approved, 4=Adjusted
-- ============================================

DO $$
DECLARE
    v_tenant_id UUID := 'e9006ab8-257f-4021-b60a-cbba785bad46';
    v_warehouse_id UUID := 'ae5e6339-6212-4435-89a9-f5940815fe20';
    v_zone_pck_a UUID := 'a5555555-5555-5555-5555-555555555555';
    v_location_ids UUID[];
    v_product_ids UUID[];
    v_product_skus TEXT[];
    v_session_id UUID;
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "CycleCountSessions" WHERE "TenantId" = v_tenant_id LIMIT 1) THEN
        -- Get pick locations
        SELECT ARRAY_AGG("Id" ORDER BY "Code")
        INTO v_location_ids
        FROM (SELECT "Id", "Code" FROM "Locations"
              WHERE "TenantId" = v_tenant_id AND "IsPickLocation" = true
              ORDER BY "Code" LIMIT 10) l;

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

        -- Session 1: Complete cycle count
        v_session_id := gen_random_uuid();
        INSERT INTO "CycleCountSessions" (
            "Id", "TenantId", "CountNumber", "Description",
            "WarehouseId", "ZoneId",
            "CountType", "CountScope", "Status",
            "ScheduledDate", "StartedAt", "CompletedAt",
            "TotalLocations", "CountedLocations", "VarianceCount",
            "RequireBlindCount", "AllowRecounts", "MaxRecounts",
            "Notes", "CreatedAt"
        ) VALUES (
            v_session_id, v_tenant_id, 'CC-2024-0001', 'Weekly Pick Zone Count',
            v_warehouse_id, v_zone_pck_a,
            0, 2, 4, -- Scheduled, Zone, Complete
            NOW() - INTERVAL '7 days', NOW() - INTERVAL '7 days', NOW() - INTERVAL '7 days',
            5, 5, 1,
            false, true, 3,
            'Regular weekly count completed', NOW()
        );

        INSERT INTO "CycleCountItems" (
            "Id", "TenantId", "CycleCountSessionId", "LocationId",
            "ProductId", "Sku",
            "ExpectedQuantity", "ExpectedBatchNumber",
            "CountedQuantity", "CountedBatchNumber", "CountedAt",
            "Status", "RecountNumber", "RequiresApproval", "IsApproved",
            "Notes", "CreatedAt"
        ) VALUES
        (gen_random_uuid(), v_tenant_id, v_session_id, v_location_ids[1], v_product_ids[1], v_product_skus[1],
         100, 'BATCH-001', 100, 'BATCH-001', NOW() - INTERVAL '7 days',
         3, 0, false, true, 'Match', NOW()), -- Approved
        (gen_random_uuid(), v_tenant_id, v_session_id, v_location_ids[2], v_product_ids[2], v_product_skus[2],
         50, 'BATCH-002', 50, 'BATCH-002', NOW() - INTERVAL '7 days',
         3, 0, false, true, 'Match', NOW()), -- Approved
        (gen_random_uuid(), v_tenant_id, v_session_id, v_location_ids[3], v_product_ids[3], v_product_skus[3],
         75, NULL, 73, NULL, NOW() - INTERVAL '7 days',
         4, 1, true, true, 'Variance found - adjusted', NOW()), -- Adjusted
        (gen_random_uuid(), v_tenant_id, v_session_id, v_location_ids[4], v_product_ids[4], v_product_skus[4],
         30, NULL, 30, NULL, NOW() - INTERVAL '7 days',
         3, 0, false, true, 'Match', NOW()), -- Approved
        (gen_random_uuid(), v_tenant_id, v_session_id, v_location_ids[5], v_product_ids[5], v_product_skus[5],
         60, NULL, 60, NULL, NOW() - INTERVAL '7 days',
         3, 0, false, true, 'Match', NOW()); -- Approved

        -- Session 2: In Progress
        v_session_id := gen_random_uuid();
        INSERT INTO "CycleCountSessions" (
            "Id", "TenantId", "CountNumber", "Description",
            "WarehouseId", "ZoneId",
            "CountType", "CountScope", "Status",
            "ScheduledDate", "StartedAt",
            "TotalLocations", "CountedLocations", "VarianceCount",
            "RequireBlindCount", "AllowRecounts", "MaxRecounts",
            "Notes", "CreatedAt"
        ) VALUES (
            v_session_id, v_tenant_id, 'CC-2024-0002', 'Mid-week Spot Check',
            v_warehouse_id, v_zone_pck_a,
            1, 0, 2, -- Random, Location, InProgress
            NOW() - INTERVAL '1 day', NOW() - INTERVAL '2 hours',
            4, 2, 0,
            true, true, 2,
            'Random spot check in progress', NOW()
        );

        INSERT INTO "CycleCountItems" (
            "Id", "TenantId", "CycleCountSessionId", "LocationId",
            "ProductId", "Sku",
            "ExpectedQuantity",
            "CountedQuantity", "CountedAt",
            "Status", "RecountNumber", "RequiresApproval", "IsApproved",
            "CreatedAt"
        ) VALUES
        (gen_random_uuid(), v_tenant_id, v_session_id, v_location_ids[6], v_product_ids[6], v_product_skus[6],
         45, 45, NOW() - INTERVAL '1 hour', 1, 0, false, false, NOW()), -- Counted
        (gen_random_uuid(), v_tenant_id, v_session_id, v_location_ids[7], v_product_ids[7], v_product_skus[7],
         80, 82, NOW() - INTERVAL '30 minutes', 1, 0, true, false, NOW()), -- Counted - variance
        (gen_random_uuid(), v_tenant_id, v_session_id, v_location_ids[8], v_product_ids[8], v_product_skus[8],
         25, NULL, NULL, 0, 0, false, false, NOW()), -- Pending
        (gen_random_uuid(), v_tenant_id, v_session_id, v_location_ids[9], v_product_ids[9], v_product_skus[9],
         55, NULL, NULL, 0, 0, false, false, NOW()); -- Pending

        -- Session 3: Scheduled (not started)
        v_session_id := gen_random_uuid();
        INSERT INTO "CycleCountSessions" (
            "Id", "TenantId", "CountNumber", "Description",
            "WarehouseId", "ZoneId",
            "CountType", "CountScope", "Status",
            "ScheduledDate",
            "TotalLocations", "CountedLocations", "VarianceCount",
            "RequireBlindCount", "AllowRecounts", "MaxRecounts",
            "Notes", "CreatedAt"
        ) VALUES (
            v_session_id, v_tenant_id, 'CC-2024-0003', 'End of Month Count',
            v_warehouse_id, NULL,
            0, 3, 1, -- Scheduled, Warehouse, Scheduled
            NOW() + INTERVAL '3 days',
            50, 0, 0,
            false, true, 3,
            'Monthly full warehouse count', NOW()
        );

        -- Session 4: Review (needs approval)
        v_session_id := gen_random_uuid();
        INSERT INTO "CycleCountSessions" (
            "Id", "TenantId", "CountNumber", "Description",
            "WarehouseId", "ZoneId",
            "CountType", "CountScope", "Status",
            "ScheduledDate", "StartedAt",
            "TotalLocations", "CountedLocations", "VarianceCount",
            "RequireBlindCount", "AllowRecounts", "MaxRecounts",
            "Notes", "CreatedAt"
        ) VALUES (
            v_session_id, v_tenant_id, 'CC-2024-0004', 'ABC A-Items Count',
            v_warehouse_id, v_zone_pck_a,
            2, 1, 3, -- ABC, Product, Review
            NOW() - INTERVAL '2 days', NOW() - INTERVAL '2 days',
            3, 3, 2,
            true, true, 3,
            'Awaiting supervisor approval for variances', NOW()
        );

        INSERT INTO "CycleCountItems" (
            "Id", "TenantId", "CycleCountSessionId", "LocationId",
            "ProductId", "Sku",
            "ExpectedQuantity",
            "CountedQuantity", "CountedAt",
            "Status", "RecountNumber", "RequiresApproval", "IsApproved",
            "Notes", "CreatedAt"
        ) VALUES
        (gen_random_uuid(), v_tenant_id, v_session_id, v_location_ids[1], v_product_ids[1], v_product_skus[1],
         100, 95, NOW() - INTERVAL '2 days', 1, 2, true, false, 'Recounted twice - variance confirmed', NOW()),
        (gen_random_uuid(), v_tenant_id, v_session_id, v_location_ids[2], v_product_ids[2], v_product_skus[2],
         50, 53, NOW() - INTERVAL '2 days', 1, 1, true, false, 'Found extra items', NOW()),
        (gen_random_uuid(), v_tenant_id, v_session_id, v_location_ids[3], v_product_ids[3], v_product_skus[3],
         75, 75, NOW() - INTERVAL '2 days', 3, 0, false, true, 'Match - auto approved', NOW());

        -- Session 5: Draft
        INSERT INTO "CycleCountSessions" (
            "Id", "TenantId", "CountNumber", "Description",
            "WarehouseId",
            "CountType", "CountScope", "Status",
            "TotalLocations", "CountedLocations", "VarianceCount",
            "RequireBlindCount", "AllowRecounts", "MaxRecounts",
            "Notes", "CreatedAt"
        ) VALUES (
            gen_random_uuid(), v_tenant_id, 'CC-2024-0005', 'Planning next count',
            v_warehouse_id,
            4, 3, 0, -- Annual, Warehouse, Draft
            0, 0, 0,
            false, true, 3,
            'Draft - planning annual count', NOW()
        );

        RAISE NOTICE 'Successfully inserted 5 cycle count sessions with items';
    ELSE
        RAISE NOTICE 'Cycle count sessions already exist, skipping seed';
    END IF;
END $$;

-- Useful queries:
-- SELECT * FROM "CycleCountSessions" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
-- SELECT ccs.*, cci.* FROM "CycleCountSessions" ccs LEFT JOIN "CycleCountItems" cci ON ccs."Id" = cci."CycleCountSessionId" WHERE ccs."TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
