-- ============================================
-- SmartWMS - Seed Fulfillment Data
-- PostgreSQL
-- ============================================
-- TenantId: e9006ab8-257f-4021-b60a-cbba785bad46
-- WarehouseId: ae5e6339-6212-4435-89a9-f5940815fe20
-- ============================================
-- FulfillmentStatus enum: 0=Created, 1=Released, 2=InProgress, 3=PartiallyComplete, 4=Complete, 5=Cancelled
-- BatchType enum: 0=Single, 1=Multi, 2=Zone, 3=Wave
-- PickTaskStatus enum: 0=Pending, 1=Assigned, 2=InProgress, 3=Complete, 4=ShortPicked, 5=Cancelled
-- ============================================

DO $$
DECLARE
    v_tenant_id UUID := 'e9006ab8-257f-4021-b60a-cbba785bad46';
    v_warehouse_id UUID := 'ae5e6339-6212-4435-89a9-f5940815fe20';
    v_zone_pck_a UUID := 'a5555555-5555-5555-5555-555555555555';
    v_zone_pak UUID := 'a7777777-7777-7777-7777-777777777777';
    v_sales_order_ids UUID[];
    v_sales_order_line_ids UUID[];
    v_product_ids UUID[];
    v_product_skus TEXT[];
    v_pick_location_ids UUID[];
    v_staging_location_id UUID;
    v_batch_id UUID;
    v_fulfillment_order_id UUID;
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "FulfillmentBatches" WHERE "TenantId" = v_tenant_id LIMIT 1) THEN

        -- Get sales orders
        SELECT ARRAY_AGG("Id" ORDER BY "OrderNumber")
        INTO v_sales_order_ids
        FROM (SELECT "Id", "OrderNumber" FROM "SalesOrders"
              WHERE "TenantId" = v_tenant_id AND "Status" IN ('Pending', 'Confirmed', 'Allocated')
              ORDER BY "OrderNumber" LIMIT 5) so;

        -- Get sales order line IDs
        SELECT ARRAY_AGG("Id" ORDER BY "Id")
        INTO v_sales_order_line_ids
        FROM (SELECT sol."Id" FROM "SalesOrderLines" sol
              INNER JOIN "SalesOrders" so ON sol."OrderId" = so."Id"
              WHERE so."TenantId" = v_tenant_id
              ORDER BY sol."Id" LIMIT 10) sol;

        -- Get products
        SELECT ARRAY_AGG("Id" ORDER BY "Sku"), ARRAY_AGG("Sku" ORDER BY "Sku")
        INTO v_product_ids, v_product_skus
        FROM (SELECT "Id", "Sku" FROM "Products"
              WHERE "TenantId" = v_tenant_id AND "IsActive" = true
              ORDER BY "Sku" LIMIT 10) p;

        -- Get pick locations
        SELECT ARRAY_AGG("Id" ORDER BY "Code")
        INTO v_pick_location_ids
        FROM (SELECT "Id", "Code" FROM "Locations"
              WHERE "TenantId" = v_tenant_id AND "IsPickLocation" = true
              ORDER BY "Code" LIMIT 10) l;

        -- Get staging location
        SELECT "Id" INTO v_staging_location_id
        FROM "Locations" WHERE "TenantId" = v_tenant_id AND "Code" = 'PACK-01' LIMIT 1;

        IF v_sales_order_ids IS NULL OR v_product_ids IS NULL OR v_pick_location_ids IS NULL THEN
            RAISE NOTICE 'Missing data for fulfillment';
            RETURN;
        END IF;

        -- Batch 1: Complete batch
        v_batch_id := gen_random_uuid();
        INSERT INTO "FulfillmentBatches" (
            "Id", "TenantId", "BatchNumber", "Name", "WarehouseId",
            "Status", "BatchType", "OrderCount", "LineCount",
            "TotalQuantity", "PickedQuantity", "Priority",
            "ZoneId", "ReleasedAt", "StartedAt", "CompletedAt",
            "Notes", "CreatedAt"
        ) VALUES (
            v_batch_id, v_tenant_id, 'BATCH-2024-0001', 'Morning Wave 1', v_warehouse_id,
            4, 3, 2, 4, 50, 50, 5, -- Complete, Wave
            v_zone_pck_a, NOW() - INTERVAL '3 days', NOW() - INTERVAL '3 days', NOW() - INTERVAL '3 days',
            'Completed successfully', NOW()
        );

        -- Add fulfillment orders and pick tasks for batch 1
        IF array_length(v_sales_order_ids, 1) >= 1 THEN
            v_fulfillment_order_id := gen_random_uuid();
            INSERT INTO "FulfillmentOrders" ("Id", "TenantId", "BatchId", "OrderId", "Sequence", "CreatedAt")
            VALUES (v_fulfillment_order_id, v_tenant_id, v_batch_id, v_sales_order_ids[1], 1, NOW());

            IF v_sales_order_line_ids IS NOT NULL AND array_length(v_sales_order_line_ids, 1) >= 2 THEN
                INSERT INTO "PickTasks" (
                    "Id", "TenantId", "TaskNumber", "BatchId", "OrderId", "OrderLineId",
                    "ProductId", "Sku", "FromLocationId", "ToLocationId",
                    "QuantityRequired", "QuantityPicked", "QuantityShortPicked",
                    "Status", "Priority", "Sequence", "StartedAt", "CompletedAt", "CreatedAt"
                ) VALUES
                (gen_random_uuid(), v_tenant_id, 'PICK-2024-00001', v_batch_id, v_sales_order_ids[1], v_sales_order_line_ids[1],
                 v_product_ids[1], v_product_skus[1], v_pick_location_ids[1], v_staging_location_id,
                 15, 15, 0, 'Complete', 5, 1, NOW() - INTERVAL '3 days', NOW() - INTERVAL '3 days', NOW()),
                (gen_random_uuid(), v_tenant_id, 'PICK-2024-00002', v_batch_id, v_sales_order_ids[1], v_sales_order_line_ids[2],
                 v_product_ids[2], v_product_skus[2], v_pick_location_ids[2], v_staging_location_id,
                 10, 10, 0, 'Complete', 5, 2, NOW() - INTERVAL '3 days', NOW() - INTERVAL '3 days', NOW());
            END IF;
        END IF;

        -- Batch 2: In Progress batch
        v_batch_id := gen_random_uuid();
        INSERT INTO "FulfillmentBatches" (
            "Id", "TenantId", "BatchNumber", "Name", "WarehouseId",
            "Status", "BatchType", "OrderCount", "LineCount",
            "TotalQuantity", "PickedQuantity", "Priority",
            "ZoneId", "ReleasedAt", "StartedAt",
            "Notes", "CreatedAt"
        ) VALUES (
            v_batch_id, v_tenant_id, 'BATCH-2024-0002', 'Afternoon Wave', v_warehouse_id,
            2, 3, 3, 6, 75, 30, 5, -- InProgress, Wave
            v_zone_pck_a, NOW() - INTERVAL '2 hours', NOW() - INTERVAL '1 hour',
            'In progress - high priority', NOW()
        );

        IF array_length(v_sales_order_ids, 1) >= 2 AND v_sales_order_line_ids IS NOT NULL AND array_length(v_sales_order_line_ids, 1) >= 5 THEN
            INSERT INTO "FulfillmentOrders" ("Id", "TenantId", "BatchId", "OrderId", "Sequence", "CreatedAt")
            VALUES (gen_random_uuid(), v_tenant_id, v_batch_id, v_sales_order_ids[2], 1, NOW());

            INSERT INTO "PickTasks" (
                "Id", "TenantId", "TaskNumber", "BatchId", "OrderId", "OrderLineId",
                "ProductId", "Sku", "FromLocationId", "ToLocationId",
                "QuantityRequired", "QuantityPicked", "QuantityShortPicked",
                "Status", "Priority", "Sequence", "StartedAt", "CreatedAt"
            ) VALUES
            (gen_random_uuid(), v_tenant_id, 'PICK-2024-00003', v_batch_id, v_sales_order_ids[2], v_sales_order_line_ids[3],
             v_product_ids[3], v_product_skus[3], v_pick_location_ids[3], v_staging_location_id,
             20, 20, 0, 'Complete', 5, 1, NOW() - INTERVAL '30 minutes', NOW()),
            (gen_random_uuid(), v_tenant_id, 'PICK-2024-00004', v_batch_id, v_sales_order_ids[2], v_sales_order_line_ids[4],
             v_product_ids[4], v_product_skus[4], v_pick_location_ids[4], v_staging_location_id,
             15, 10, 0, 'InProgress', 5, 2, NOW() - INTERVAL '15 minutes', NOW()),
            (gen_random_uuid(), v_tenant_id, 'PICK-2024-00005', v_batch_id, v_sales_order_ids[2], v_sales_order_line_ids[5],
             v_product_ids[5], v_product_skus[5], v_pick_location_ids[5], v_staging_location_id,
             25, 0, 0, 'Pending', 5, 3, NULL, NOW());
        END IF;

        -- Batch 3: Released but not started
        v_batch_id := gen_random_uuid();
        INSERT INTO "FulfillmentBatches" (
            "Id", "TenantId", "BatchNumber", "Name", "WarehouseId",
            "Status", "BatchType", "OrderCount", "LineCount",
            "TotalQuantity", "PickedQuantity", "Priority",
            "ZoneId", "ReleasedAt",
            "Notes", "CreatedAt"
        ) VALUES (
            v_batch_id, v_tenant_id, 'BATCH-2024-0003', 'Evening Express', v_warehouse_id,
            1, 1, 2, 3, 40, 0, 8, -- Released, Multi
            v_zone_pck_a, NOW() - INTERVAL '30 minutes',
            'Express orders - urgent', NOW()
        );

        IF array_length(v_sales_order_ids, 1) >= 3 AND v_sales_order_line_ids IS NOT NULL AND array_length(v_sales_order_line_ids, 1) >= 7 THEN
            INSERT INTO "FulfillmentOrders" ("Id", "TenantId", "BatchId", "OrderId", "Sequence", "CreatedAt")
            VALUES (gen_random_uuid(), v_tenant_id, v_batch_id, v_sales_order_ids[3], 1, NOW());

            INSERT INTO "PickTasks" (
                "Id", "TenantId", "TaskNumber", "BatchId", "OrderId", "OrderLineId",
                "ProductId", "Sku", "FromLocationId", "ToLocationId",
                "QuantityRequired", "QuantityPicked", "QuantityShortPicked",
                "Status", "Priority", "Sequence", "CreatedAt"
            ) VALUES
            (gen_random_uuid(), v_tenant_id, 'PICK-2024-00006', v_batch_id, v_sales_order_ids[3], v_sales_order_line_ids[6],
             v_product_ids[6], v_product_skus[6], v_pick_location_ids[6], v_staging_location_id,
             20, 0, 0, 'Pending', 8, 1, NOW()),
            (gen_random_uuid(), v_tenant_id, 'PICK-2024-00007', v_batch_id, v_sales_order_ids[3], v_sales_order_line_ids[7],
             v_product_ids[7], v_product_skus[7], v_pick_location_ids[7], v_staging_location_id,
             20, 0, 0, 'Pending', 8, 2, NOW());
        END IF;

        -- Batch 4: Created (draft)
        v_batch_id := gen_random_uuid();
        INSERT INTO "FulfillmentBatches" (
            "Id", "TenantId", "BatchNumber", "Name", "WarehouseId",
            "Status", "BatchType", "OrderCount", "LineCount",
            "TotalQuantity", "PickedQuantity", "Priority",
            "Notes", "CreatedAt"
        ) VALUES (
            v_batch_id, v_tenant_id, 'BATCH-2024-0004', 'Tomorrow Batch', v_warehouse_id,
            0, 0, 1, 2, 30, 0, 5, -- Created, Single
            'Draft - pending review', NOW()
        );

        -- Batch 5: Partially Complete with short pick
        v_batch_id := gen_random_uuid();
        INSERT INTO "FulfillmentBatches" (
            "Id", "TenantId", "BatchNumber", "Name", "WarehouseId",
            "Status", "BatchType", "OrderCount", "LineCount",
            "TotalQuantity", "PickedQuantity", "Priority",
            "ZoneId", "ReleasedAt", "StartedAt",
            "Notes", "CreatedAt"
        ) VALUES (
            v_batch_id, v_tenant_id, 'BATCH-2024-0005', 'Problem Batch', v_warehouse_id,
            3, 1, 1, 2, 50, 35, 5, -- PartiallyComplete, Multi
            v_zone_pck_a, NOW() - INTERVAL '4 hours', NOW() - INTERVAL '3 hours',
            'Short pick on one item', NOW()
        );

        IF array_length(v_sales_order_ids, 1) >= 4 AND v_sales_order_line_ids IS NOT NULL AND array_length(v_sales_order_line_ids, 1) >= 9 THEN
            INSERT INTO "FulfillmentOrders" ("Id", "TenantId", "BatchId", "OrderId", "Sequence", "CreatedAt")
            VALUES (gen_random_uuid(), v_tenant_id, v_batch_id, v_sales_order_ids[4], 1, NOW());

            INSERT INTO "PickTasks" (
                "Id", "TenantId", "TaskNumber", "BatchId", "OrderId", "OrderLineId",
                "ProductId", "Sku", "FromLocationId", "ToLocationId",
                "QuantityRequired", "QuantityPicked", "QuantityShortPicked",
                "Status", "Priority", "Sequence", "ShortPickReason", "CompletedAt", "CreatedAt"
            ) VALUES
            (gen_random_uuid(), v_tenant_id, 'PICK-2024-00008', v_batch_id, v_sales_order_ids[4], v_sales_order_line_ids[8],
             v_product_ids[8], v_product_skus[8], v_pick_location_ids[8], v_staging_location_id,
             25, 25, 0, 'Complete', 5, 1, NULL, NOW() - INTERVAL '2 hours', NOW()),
            (gen_random_uuid(), v_tenant_id, 'PICK-2024-00009', v_batch_id, v_sales_order_ids[4], v_sales_order_line_ids[9],
             v_product_ids[9], v_product_skus[9], v_pick_location_ids[9], v_staging_location_id,
             25, 10, 15, 'ShortPicked', 5, 2, 'Insufficient stock at location', NOW() - INTERVAL '1 hour', NOW());
        END IF;

        RAISE NOTICE 'Successfully inserted 5 fulfillment batches with orders and pick tasks';
    ELSE
        RAISE NOTICE 'Fulfillment batches already exist, skipping seed';
    END IF;
END $$;

-- FulfillmentStatus enum reference:
-- 0 = Created, 1 = Released, 2 = InProgress, 3 = PartiallyComplete, 4 = Complete, 5 = Cancelled

-- BatchType enum reference:
-- 0 = Single, 1 = Multi, 2 = Zone, 3 = Wave

-- PickTaskStatus enum reference:
-- 0 = Pending, 1 = Assigned, 2 = InProgress, 3 = Complete, 4 = ShortPicked, 5 = Cancelled

-- Useful queries:
-- SELECT * FROM "FulfillmentBatches" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
-- SELECT fb.*, fo.*, pt.* FROM "FulfillmentBatches" fb LEFT JOIN "FulfillmentOrders" fo ON fb."Id" = fo."BatchId" LEFT JOIN "PickTasks" pt ON fb."Id" = pt."BatchId" WHERE fb."TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
