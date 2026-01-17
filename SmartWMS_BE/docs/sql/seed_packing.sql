-- ============================================
-- SmartWMS - Seed Packing Data
-- PostgreSQL
-- ============================================
-- TenantId: e9006ab8-257f-4021-b60a-cbba785bad46
-- WarehouseId: ae5e6339-6212-4435-89a9-f5940815fe20
-- ============================================
-- PackingTaskStatus enum: 0=Pending, 1=Assigned, 2=InProgress, 3=Complete, 4=Cancelled
-- ============================================

DO $$
DECLARE
    v_tenant_id UUID := 'e9006ab8-257f-4021-b60a-cbba785bad46';
    v_warehouse_id UUID := 'ae5e6339-6212-4435-89a9-f5940815fe20';
    v_sales_order_ids UUID[];
    v_product_ids UUID[];
    v_product_skus TEXT[];
    v_station_id UUID;
    v_task_id UUID;
    v_package_id UUID;
BEGIN
    -- Create Packing Stations first
    IF NOT EXISTS (SELECT 1 FROM "PackingStations" WHERE "TenantId" = v_tenant_id LIMIT 1) THEN
        INSERT INTO "PackingStations" (
            "Id", "TenantId", "Code", "Name", "WarehouseId",
            "IsActive", "CanPrintLabels", "HasScale", "HasDimensioner",
            "Notes", "CreatedAt"
        ) VALUES
        ('44444444-0001-0001-0001-000000000001', v_tenant_id, 'PACK-ST-01', 'Packing Station 1', v_warehouse_id,
         true, true, true, true, 'Main packing station with full capabilities', NOW()),
        ('44444444-0001-0001-0002-000000000001', v_tenant_id, 'PACK-ST-02', 'Packing Station 2', v_warehouse_id,
         true, true, true, false, 'Standard packing station', NOW()),
        ('44444444-0001-0001-0003-000000000001', v_tenant_id, 'PACK-ST-03', 'Packing Station 3', v_warehouse_id,
         true, true, false, false, 'Basic packing station', NOW()),
        ('44444444-0001-0001-0004-000000000001', v_tenant_id, 'PACK-ST-04', 'Packing Station 4', v_warehouse_id,
         true, true, true, true, 'Express packing station', NOW()),
        ('44444444-0001-0001-0005-000000000001', v_tenant_id, 'PACK-ST-05', 'Packing Station 5 (Inactive)', v_warehouse_id,
         false, true, false, false, 'Currently under maintenance', NOW());

        RAISE NOTICE 'Successfully inserted 5 packing stations';
    END IF;

    v_station_id := '44444444-0001-0001-0001-000000000001';

    IF NOT EXISTS (SELECT 1 FROM "PackingTasks" WHERE "TenantId" = v_tenant_id LIMIT 1) THEN
        -- Get sales orders
        SELECT ARRAY_AGG("Id" ORDER BY "OrderNumber")
        INTO v_sales_order_ids
        FROM (SELECT "Id", "OrderNumber" FROM "SalesOrders"
              WHERE "TenantId" = v_tenant_id ORDER BY "OrderNumber" LIMIT 5) so;

        -- Get products
        SELECT ARRAY_AGG("Id" ORDER BY "Sku"), ARRAY_AGG("Sku" ORDER BY "Sku")
        INTO v_product_ids, v_product_skus
        FROM (SELECT "Id", "Sku" FROM "Products"
              WHERE "TenantId" = v_tenant_id AND "IsActive" = true ORDER BY "Sku" LIMIT 10) p;

        IF v_sales_order_ids IS NULL THEN
            RAISE NOTICE 'No sales orders found';
            RETURN;
        END IF;

        -- Task 1: Complete packing task
        v_task_id := gen_random_uuid();
        INSERT INTO "PackingTasks" (
            "Id", "TenantId", "TaskNumber", "SalesOrderId", "FulfillmentBatchId",
            "PackingStationId", "AssignedToUserId", "AssignedAt",
            "Status", "TotalItems", "PackedItems", "BoxCount", "TotalWeightKg",
            "StartedAt", "CompletedAt", "Priority", "Notes", "CreatedAt"
        ) VALUES (
            v_task_id, v_tenant_id, 'PACK-2024-00001', v_sales_order_ids[1], NULL,
            v_station_id, NULL, NULL,
            3, 25, 25, 2, 8.5, -- Complete
            NOW() - INTERVAL '2 days', NOW() - INTERVAL '2 days', 5, 'Standard packing', NOW()
        );

        -- Add packages for task 1
        v_package_id := gen_random_uuid();
        INSERT INTO "Packages" (
            "Id", "TenantId", "PackingTaskId", "PackageNumber", "SequenceNumber",
            "LengthMm", "WidthMm", "HeightMm", "WeightKg",
            "PackagingType", "TrackingNumber", "LabelUrl", "CreatedAt"
        ) VALUES
        (v_package_id, v_tenant_id, v_task_id, 'PKG-2024-00001-01', 1,
         400, 300, 200, 5.2, 'Box', 'JD014600006753906201', NULL, NOW()),
        (gen_random_uuid(), v_tenant_id, v_task_id, 'PKG-2024-00001-02', 2,
         300, 250, 150, 3.3, 'Box', 'JD014600006753906202', NULL, NOW());

        -- Add package items
        INSERT INTO "PackageItems" (
            "Id", "TenantId", "PackageId", "ProductId", "Sku", "Quantity", "CreatedAt"
        ) VALUES
        (gen_random_uuid(), v_tenant_id, v_package_id, v_product_ids[1], v_product_skus[1], 15, NOW()),
        (gen_random_uuid(), v_tenant_id, v_package_id, v_product_ids[2], v_product_skus[2], 10, NOW());

        -- Task 2: In Progress
        v_task_id := gen_random_uuid();
        INSERT INTO "PackingTasks" (
            "Id", "TenantId", "TaskNumber", "SalesOrderId", "FulfillmentBatchId",
            "PackingStationId", "AssignedToUserId", "AssignedAt",
            "Status", "TotalItems", "PackedItems", "BoxCount", "TotalWeightKg",
            "StartedAt", "Priority", "Notes", "CreatedAt"
        ) VALUES (
            v_task_id, v_tenant_id, 'PACK-2024-00002', v_sales_order_ids[2], NULL,
            v_station_id, NULL, NOW() - INTERVAL '30 minutes',
            2, 15, 8, 1, 3.2, -- InProgress
            NOW() - INTERVAL '20 minutes', 5, 'Packing in progress', NOW()
        );

        v_package_id := gen_random_uuid();
        INSERT INTO "Packages" (
            "Id", "TenantId", "PackingTaskId", "PackageNumber", "SequenceNumber",
            "LengthMm", "WidthMm", "HeightMm", "WeightKg", "PackagingType", "CreatedAt"
        ) VALUES (v_package_id, v_tenant_id, v_task_id, 'PKG-2024-00002-01', 1,
         350, 280, 180, 3.2, 'Box', NOW());

        INSERT INTO "PackageItems" ("Id", "TenantId", "PackageId", "ProductId", "Sku", "Quantity", "CreatedAt")
        VALUES (gen_random_uuid(), v_tenant_id, v_package_id, v_product_ids[3], v_product_skus[3], 8, NOW());

        -- Task 3: Assigned but not started
        INSERT INTO "PackingTasks" (
            "Id", "TenantId", "TaskNumber", "SalesOrderId",
            "PackingStationId", "AssignedAt",
            "Status", "TotalItems", "PackedItems", "BoxCount", "TotalWeightKg",
            "Priority", "Notes", "CreatedAt"
        ) VALUES (
            gen_random_uuid(), v_tenant_id, 'PACK-2024-00003', v_sales_order_ids[3],
            '44444444-0001-0001-0002-000000000001', NOW() - INTERVAL '10 minutes',
            1, 20, 0, 0, 0, -- Assigned
            8, 'High priority - express order', NOW()
        );

        -- Task 4: Pending
        INSERT INTO "PackingTasks" (
            "Id", "TenantId", "TaskNumber", "SalesOrderId",
            "Status", "TotalItems", "PackedItems", "BoxCount", "TotalWeightKg",
            "Priority", "CreatedAt"
        ) VALUES (
            gen_random_uuid(), v_tenant_id, 'PACK-2024-00004', v_sales_order_ids[4],
            0, 30, 0, 0, 0, -- Pending
            5, NOW()
        );

        -- Task 5: Cancelled
        INSERT INTO "PackingTasks" (
            "Id", "TenantId", "TaskNumber", "SalesOrderId",
            "Status", "TotalItems", "PackedItems", "BoxCount", "TotalWeightKg",
            "Priority", "Notes", "CreatedAt"
        ) VALUES (
            gen_random_uuid(), v_tenant_id, 'PACK-2024-00005', v_sales_order_ids[5],
            4, 12, 0, 0, 0, -- Cancelled
            5, 'Order cancelled by customer', NOW()
        );

        RAISE NOTICE 'Successfully inserted 5 packing tasks with packages';
    ELSE
        RAISE NOTICE 'Packing tasks already exist, skipping seed';
    END IF;
END $$;

-- PackingTaskStatus enum reference:
-- 0 = Pending, 1 = Assigned, 2 = InProgress, 3 = Complete, 4 = Cancelled

-- Useful queries:
-- SELECT * FROM "PackingStations" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
-- SELECT * FROM "PackingTasks" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
-- SELECT pt.*, pkg.*, pi.* FROM "PackingTasks" pt LEFT JOIN "Packages" pkg ON pt."Id" = pkg."PackingTaskId" LEFT JOIN "PackageItems" pi ON pkg."Id" = pi."PackageId" WHERE pt."TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
