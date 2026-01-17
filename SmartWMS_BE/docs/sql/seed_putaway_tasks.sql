-- ============================================
-- SmartWMS - Seed Putaway Tasks Data
-- PostgreSQL
-- ============================================
-- TenantId: e9006ab8-257f-4021-b60a-cbba785bad46
-- ============================================
-- PutawayTaskStatus enum: 0=Pending, 1=Assigned, 2=InProgress, 3=Complete, 4=Cancelled
-- ============================================

DO $$
DECLARE
    v_tenant_id UUID := 'e9006ab8-257f-4021-b60a-cbba785bad46';
    v_product_ids UUID[];
    v_product_skus TEXT[];
    v_from_location_id UUID;
    v_to_location_ids UUID[];
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "PutawayTasks" WHERE "TenantId" = v_tenant_id LIMIT 1) THEN

        -- Get receiving dock location
        SELECT "Id" INTO v_from_location_id
        FROM "Locations" WHERE "TenantId" = v_tenant_id AND "Code" = 'STG-IN-01' LIMIT 1;

        -- Get bulk storage locations
        SELECT ARRAY_AGG("Id" ORDER BY "Code")
        INTO v_to_location_ids
        FROM (SELECT "Id", "Code" FROM "Locations"
              WHERE "TenantId" = v_tenant_id AND "Code" LIKE 'A01%' AND "IsPutawayLocation" = true
              ORDER BY "Code" LIMIT 10) l;

        -- Get products
        SELECT ARRAY_AGG("Id" ORDER BY "Sku"), ARRAY_AGG("Sku" ORDER BY "Sku")
        INTO v_product_ids, v_product_skus
        FROM (SELECT "Id", "Sku" FROM "Products"
              WHERE "TenantId" = v_tenant_id AND "IsActive" = true
              ORDER BY "Sku" LIMIT 10) p;

        IF v_from_location_id IS NULL OR v_to_location_ids IS NULL OR v_product_ids IS NULL THEN
            RAISE NOTICE 'Missing data for putaway tasks';
            RETURN;
        END IF;

        INSERT INTO "PutawayTasks" (
            "Id", "TenantId", "TaskNumber",
            "GoodsReceiptId", "GoodsReceiptLineId",
            "ProductId", "Sku",
            "QuantityToPutaway", "QuantityPutaway",
            "FromLocationId", "SuggestedLocationId", "ActualLocationId",
            "BatchNumber", "ExpiryDate",
            "AssignedToUserId", "AssignedAt",
            "Status", "StartedAt", "CompletedAt",
            "Priority", "Notes", "CreatedAt"
        ) VALUES
        -- Completed putaway tasks
        (gen_random_uuid(), v_tenant_id, 'PUT-2024-00001', NULL, NULL, v_product_ids[1], v_product_skus[1],
         50, 50, v_from_location_id, v_to_location_ids[1], v_to_location_ids[1],
         'BATCH-2024-001', NOW() + INTERVAL '180 days', NULL, NULL,
         3, NOW() - INTERVAL '5 days', NOW() - INTERVAL '5 days', 5, 'Standard putaway', NOW()),

        (gen_random_uuid(), v_tenant_id, 'PUT-2024-00002', NULL, NULL, v_product_ids[2], v_product_skus[2],
         30, 30, v_from_location_id, v_to_location_ids[2], v_to_location_ids[2],
         'BATCH-2024-002', NOW() + INTERVAL '180 days', NULL, NULL,
         3, NOW() - INTERVAL '4 days', NOW() - INTERVAL '4 days', 5, NULL, NOW()),

        (gen_random_uuid(), v_tenant_id, 'PUT-2024-00003', NULL, NULL, v_product_ids[3], v_product_skus[3],
         40, 40, v_from_location_id, v_to_location_ids[3], v_to_location_ids[3],
         'BATCH-2024-003', NOW() + INTERVAL '365 days', NULL, NULL,
         3, NOW() - INTERVAL '3 days', NOW() - INTERVAL '3 days', 5, NULL, NOW()),

        -- In Progress putaway tasks
        (gen_random_uuid(), v_tenant_id, 'PUT-2024-00004', NULL, NULL, v_product_ids[4], v_product_skus[4],
         60, 30, v_from_location_id, v_to_location_ids[4], NULL,
         'BATCH-2024-004', NOW() + INTERVAL '90 days', NULL, NOW() - INTERVAL '1 hour',
         2, NOW() - INTERVAL '1 hour', NULL, 3, 'Urgent - high priority items', NOW()),

        (gen_random_uuid(), v_tenant_id, 'PUT-2024-00005', NULL, NULL, v_product_ids[5], v_product_skus[5],
         25, 0, v_from_location_id, v_to_location_ids[5], NULL,
         NULL, NULL, NULL, NOW() - INTERVAL '30 minutes',
         2, NOW() - INTERVAL '30 minutes', NULL, 5, NULL, NOW()),

        -- Assigned but not started
        (gen_random_uuid(), v_tenant_id, 'PUT-2024-00006', NULL, NULL, v_product_ids[6], v_product_skus[6],
         45, 0, v_from_location_id, v_to_location_ids[6], NULL,
         'BATCH-2024-006', NOW() + INTERVAL '180 days', NULL, NOW() - INTERVAL '15 minutes',
         1, NULL, NULL, 5, NULL, NOW()),

        -- Pending putaway tasks (not assigned)
        (gen_random_uuid(), v_tenant_id, 'PUT-2024-00007', NULL, NULL, v_product_ids[7], v_product_skus[7],
         80, 0, v_from_location_id, v_to_location_ids[7], NULL,
         'BATCH-2024-007', NOW() + INTERVAL '180 days', NULL, NULL,
         0, NULL, NULL, 5, NULL, NOW()),

        (gen_random_uuid(), v_tenant_id, 'PUT-2024-00008', NULL, NULL, v_product_ids[8], v_product_skus[8],
         35, 0, v_from_location_id, v_to_location_ids[8], NULL,
         NULL, NULL, NULL, NULL,
         0, NULL, NULL, 5, NULL, NOW()),

        (gen_random_uuid(), v_tenant_id, 'PUT-2024-00009', NULL, NULL, v_product_ids[9], v_product_skus[9],
         100, 0, v_from_location_id, v_to_location_ids[9], NULL,
         'BATCH-2024-009', NOW() + INTERVAL '365 days', NULL, NULL,
         0, NULL, NULL, 2, 'Bulk putaway - large quantity', NOW()),

        -- Cancelled task
        (gen_random_uuid(), v_tenant_id, 'PUT-2024-00010', NULL, NULL, v_product_ids[10], v_product_skus[10],
         20, 0, v_from_location_id, v_to_location_ids[10], NULL,
         NULL, NULL, NULL, NULL,
         4, NULL, NULL, 5, 'Cancelled - goods returned to supplier', NOW());

        RAISE NOTICE 'Successfully inserted 10 putaway tasks';
    ELSE
        RAISE NOTICE 'Putaway tasks already exist, skipping seed';
    END IF;
END $$;

-- PutawayTaskStatus enum reference:
-- 0 = Pending, 1 = Assigned, 2 = InProgress, 3 = Complete, 4 = Cancelled

-- Useful queries:
-- SELECT * FROM "PutawayTasks" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
-- SELECT * FROM "PutawayTasks" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46' AND "Status" = 0; -- Pending
