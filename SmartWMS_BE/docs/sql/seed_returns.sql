-- ============================================
-- SmartWMS - Seed Returns Data
-- PostgreSQL
-- ============================================
-- TenantId: e9006ab8-257f-4021-b60a-cbba785bad46
-- ============================================
-- ReturnOrderStatus enum: 0=Pending, 1=InTransit, 2=Received, 3=InProgress, 4=Complete, 5=Cancelled
-- ReturnType enum: 0=CustomerReturn, 1=SupplierReturn, 2=InternalTransfer, 3=Damaged, 4=Recall
-- ReturnCondition enum: 0=Unknown, 1=Good, 2=Refurbished, 3=Damaged, 4=Defective, 5=Destroyed
-- ReturnDisposition enum: 0=Pending, 1=ReturnToStock, 2=Quarantine, 3=Scrap, 4=ReturnToSupplier, 5=Donate, 6=Repair
-- ============================================

DO $$
DECLARE
    v_tenant_id UUID := 'e9006ab8-257f-4021-b60a-cbba785bad46';
    v_customer_ids UUID[];
    v_product_ids UUID[];
    v_product_skus TEXT[];
    v_rcv_location_id UUID;
    v_return_id UUID;
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "ReturnOrders" WHERE "TenantId" = v_tenant_id LIMIT 1) THEN
        -- Get receiving location
        SELECT "Id" INTO v_rcv_location_id
        FROM "Locations" WHERE "TenantId" = v_tenant_id AND "Code" LIKE 'RET%' LIMIT 1;

        -- Get customers
        SELECT ARRAY_AGG("Id" ORDER BY "Code")
        INTO v_customer_ids
        FROM (SELECT "Id", "Code" FROM "Customers"
              WHERE "TenantId" = v_tenant_id AND "IsActive" = true ORDER BY "Code" LIMIT 6) c;

        -- Get products
        SELECT ARRAY_AGG("Id" ORDER BY "Sku"), ARRAY_AGG("Sku" ORDER BY "Sku")
        INTO v_product_ids, v_product_skus
        FROM (SELECT "Id", "Sku" FROM "Products"
              WHERE "TenantId" = v_tenant_id AND "IsActive" = true ORDER BY "Sku" LIMIT 10) p;

        IF v_customer_ids IS NULL OR v_product_ids IS NULL THEN
            RAISE NOTICE 'Missing customers or products';
            RETURN;
        END IF;

        -- Return 1: Complete - returned to stock
        v_return_id := gen_random_uuid();
        INSERT INTO "ReturnOrders" (
            "Id", "TenantId", "ReturnNumber", "OriginalSalesOrderId",
            "CustomerId", "Status", "ReturnType",
            "ReasonDescription", "ReceivingLocationId",
            "RequestedDate", "ReceivedDate", "ProcessedDate",
            "RmaNumber", "RmaExpiryDate",
            "CarrierCode", "TrackingNumber",
            "TotalLines", "TotalQuantityExpected", "TotalQuantityReceived",
            "Notes", "CreatedAt"
        ) VALUES (
            v_return_id, v_tenant_id, 'RET-2024-0001', NULL,
            v_customer_ids[1], 4, 0, -- Complete, CustomerReturn
            'Customer changed mind - items unopened', v_rcv_location_id,
            NOW() - INTERVAL '10 days', NOW() - INTERVAL '7 days', NOW() - INTERVAL '6 days',
            'RMA-2024-0001', NOW() + INTERVAL '30 days',
            'POSTNORD', 'SE9876543210',
            2, 15, 15,
            'Items in original packaging', NOW()
        );

        INSERT INTO "ReturnOrderLines" (
            "Id", "TenantId", "ReturnOrderId", "LineNumber",
            "ProductId", "Sku",
            "QuantityExpected", "QuantityReceived", "QuantityAccepted", "QuantityRejected",
            "Condition", "ConditionNotes", "Disposition", "DispositionLocationId",
            "ReasonDescription", "CreatedAt"
        ) VALUES
        (gen_random_uuid(), v_tenant_id, v_return_id, 1, v_product_ids[1], v_product_skus[1],
         10, 10, 10, 0, 1, 'Like new - sealed packaging', 1, v_rcv_location_id, -- Good, ReturnToStock
         'Unopened', NOW()),
        (gen_random_uuid(), v_tenant_id, v_return_id, 2, v_product_ids[2], v_product_skus[2],
         5, 5, 5, 0, 1, 'Good condition', 1, v_rcv_location_id, -- Good, ReturnToStock
         'Lightly used', NOW());

        -- Return 2: In Progress - being processed
        v_return_id := gen_random_uuid();
        INSERT INTO "ReturnOrders" (
            "Id", "TenantId", "ReturnNumber", "CustomerId",
            "Status", "ReturnType", "ReasonDescription",
            "ReceivingLocationId", "RequestedDate", "ReceivedDate",
            "RmaNumber", "TrackingNumber",
            "TotalLines", "TotalQuantityExpected", "TotalQuantityReceived",
            "Notes", "CreatedAt"
        ) VALUES (
            v_return_id, v_tenant_id, 'RET-2024-0002', v_customer_ids[2],
            3, 0, 'Defective product', -- InProgress, CustomerReturn
            v_rcv_location_id, NOW() - INTERVAL '5 days', NOW() - INTERVAL '3 days',
            'RMA-2024-0002', 'JD123456789012',
            2, 8, 8,
            'Quality check in progress', NOW()
        );

        INSERT INTO "ReturnOrderLines" (
            "Id", "TenantId", "ReturnOrderId", "LineNumber",
            "ProductId", "Sku",
            "QuantityExpected", "QuantityReceived", "QuantityAccepted", "QuantityRejected",
            "Condition", "ConditionNotes", "Disposition",
            "ReasonDescription", "CreatedAt"
        ) VALUES
        (gen_random_uuid(), v_tenant_id, v_return_id, 1, v_product_ids[3], v_product_skus[3],
         5, 5, 3, 2, 4, 'Manufacturing defect on 2 units', 0, -- Defective, Pending disposition
         'Not working as expected', NOW()),
        (gen_random_uuid(), v_tenant_id, v_return_id, 2, v_product_ids[4], v_product_skus[4],
         3, 3, 3, 0, 1, 'Good condition', 1, -- Good, ReturnToStock
         'Works fine - customer error', NOW());

        -- Return 3: Received - awaiting inspection
        v_return_id := gen_random_uuid();
        INSERT INTO "ReturnOrders" (
            "Id", "TenantId", "ReturnNumber", "CustomerId",
            "Status", "ReturnType", "ReasonDescription",
            "ReceivingLocationId", "RequestedDate", "ReceivedDate",
            "RmaNumber",
            "TotalLines", "TotalQuantityExpected", "TotalQuantityReceived",
            "Notes", "CreatedAt"
        ) VALUES (
            v_return_id, v_tenant_id, 'RET-2024-0003', v_customer_ids[3],
            2, 3, 'Damaged during shipping', -- Received, Damaged
            v_rcv_location_id, NOW() - INTERVAL '3 days', NOW() - INTERVAL '1 day',
            'RMA-2024-0003',
            1, 10, 10,
            'Awaiting damage assessment', NOW()
        );

        INSERT INTO "ReturnOrderLines" (
            "Id", "TenantId", "ReturnOrderId", "LineNumber",
            "ProductId", "Sku",
            "QuantityExpected", "QuantityReceived", "QuantityAccepted", "QuantityRejected",
            "Condition", "ConditionNotes", "Disposition",
            "ReasonDescription", "CreatedAt"
        ) VALUES
        (gen_random_uuid(), v_tenant_id, v_return_id, 1, v_product_ids[5], v_product_skus[5],
         10, 10, 0, 0, 3, 'Visible packaging damage - contents unknown', 0, -- Damaged, Pending
         'Box was crushed', NOW());

        -- Return 4: In Transit
        v_return_id := gen_random_uuid();
        INSERT INTO "ReturnOrders" (
            "Id", "TenantId", "ReturnNumber", "CustomerId",
            "Status", "ReturnType", "ReasonDescription",
            "RequestedDate",
            "RmaNumber", "RmaExpiryDate",
            "CarrierCode", "TrackingNumber",
            "TotalLines", "TotalQuantityExpected", "TotalQuantityReceived",
            "Notes", "CreatedAt"
        ) VALUES (
            v_return_id, v_tenant_id, 'RET-2024-0004', v_customer_ids[4],
            1, 0, 'Wrong size ordered', -- InTransit, CustomerReturn
            NOW() - INTERVAL '2 days',
            'RMA-2024-0004', NOW() + INTERVAL '30 days',
            'DHL', 'JD987654321',
            1, 2, 0,
            'Expected arrival tomorrow', NOW()
        );

        INSERT INTO "ReturnOrderLines" (
            "Id", "TenantId", "ReturnOrderId", "LineNumber",
            "ProductId", "Sku",
            "QuantityExpected", "QuantityReceived", "QuantityAccepted", "QuantityRejected",
            "Condition", "Disposition", "ReasonDescription", "CreatedAt"
        ) VALUES
        (gen_random_uuid(), v_tenant_id, v_return_id, 1, v_product_ids[6], v_product_skus[6],
         2, 0, 0, 0, 0, 0, 'Size L instead of M needed', NOW()); -- Unknown, Pending

        -- Return 5: Pending (RMA issued, not shipped)
        v_return_id := gen_random_uuid();
        INSERT INTO "ReturnOrders" (
            "Id", "TenantId", "ReturnNumber", "CustomerId",
            "Status", "ReturnType", "ReasonDescription",
            "RequestedDate",
            "RmaNumber", "RmaExpiryDate",
            "TotalLines", "TotalQuantityExpected", "TotalQuantityReceived",
            "Notes", "CreatedAt"
        ) VALUES (
            v_return_id, v_tenant_id, 'RET-2024-0005', v_customer_ids[5],
            0, 0, 'Product not as described', -- Pending, CustomerReturn
            NOW(),
            'RMA-2024-0005', NOW() + INTERVAL '30 days',
            1, 5, 0,
            'RMA issued - awaiting customer shipment', NOW()
        );

        INSERT INTO "ReturnOrderLines" (
            "Id", "TenantId", "ReturnOrderId", "LineNumber",
            "ProductId", "Sku",
            "QuantityExpected", "QuantityReceived", "QuantityAccepted", "QuantityRejected",
            "Condition", "Disposition", "ReasonDescription", "CreatedAt"
        ) VALUES
        (gen_random_uuid(), v_tenant_id, v_return_id, 1, v_product_ids[7], v_product_skus[7],
         5, 0, 0, 0, 0, 0, 'Color different from photo', NOW());

        -- Return 6: Cancelled
        v_return_id := gen_random_uuid();
        INSERT INTO "ReturnOrders" (
            "Id", "TenantId", "ReturnNumber", "CustomerId",
            "Status", "ReturnType", "ReasonDescription",
            "RequestedDate",
            "RmaNumber",
            "TotalLines", "TotalQuantityExpected", "TotalQuantityReceived",
            "Notes", "CreatedAt"
        ) VALUES (
            v_return_id, v_tenant_id, 'RET-2024-0006', v_customer_ids[6],
            5, 0, 'Cancelled - customer kept items', -- Cancelled, CustomerReturn
            NOW() - INTERVAL '8 days',
            'RMA-2024-0006',
            1, 3, 0,
            'Customer decided to keep the items', NOW()
        );

        INSERT INTO "ReturnOrderLines" (
            "Id", "TenantId", "ReturnOrderId", "LineNumber",
            "ProductId", "Sku",
            "QuantityExpected", "QuantityReceived", "QuantityAccepted", "QuantityRejected",
            "Condition", "Disposition", "ReasonDescription", "CreatedAt"
        ) VALUES
        (gen_random_uuid(), v_tenant_id, v_return_id, 1, v_product_ids[8], v_product_skus[8],
         3, 0, 0, 0, 0, 0, 'Cancelled by customer', NOW());

        RAISE NOTICE 'Successfully inserted 6 return orders with lines';
    ELSE
        RAISE NOTICE 'Return orders already exist, skipping seed';
    END IF;
END $$;

-- Useful queries:
-- SELECT * FROM "ReturnOrders" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
-- SELECT ro.*, rol.* FROM "ReturnOrders" ro JOIN "ReturnOrderLines" rol ON ro."Id" = rol."ReturnOrderId" WHERE ro."TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
