-- ============================================
-- SmartWMS - Seed Stock Movements Data
-- PostgreSQL
-- ============================================
-- TenantId: e9006ab8-257f-4021-b60a-cbba785bad46
-- ============================================
-- MovementType enum: 0=Receipt, 1=Issue, 2=Transfer, 3=Adjustment, 4=Return, 5=WriteOff, 6=CycleCount
-- ============================================

DO $$
DECLARE
    v_tenant_id UUID := 'e9006ab8-257f-4021-b60a-cbba785bad46';
    v_wh_stockholm UUID := 'ae5e6339-6212-4435-89a9-f5940815fe20';
    v_product_ids UUID[];
    v_product_skus TEXT[];
    v_location_ids UUID[];
    v_rcv_location_id UUID;
    v_seq INTEGER := 0;
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "StockMovements" WHERE "TenantId" = v_tenant_id LIMIT 1) THEN

        -- Get receiving location
        SELECT "Id" INTO v_rcv_location_id
        FROM "Locations"
        WHERE "TenantId" = v_tenant_id AND "Code" = 'DOCK-RCV-01'
        LIMIT 1;

        -- Get first 15 products
        SELECT ARRAY_AGG("Id" ORDER BY "Sku"), ARRAY_AGG("Sku" ORDER BY "Sku")
        INTO v_product_ids, v_product_skus
        FROM (SELECT "Id", "Sku" FROM "Products"
              WHERE "TenantId" = v_tenant_id AND "IsActive" = true
              ORDER BY "Sku" LIMIT 15) p;

        -- Get pick locations
        SELECT ARRAY_AGG("Id" ORDER BY "Code")
        INTO v_location_ids
        FROM (SELECT "Id", "Code" FROM "Locations"
              WHERE "TenantId" = v_tenant_id AND "IsPickLocation" = true
              ORDER BY "Code" LIMIT 15) l;

        IF v_product_ids IS NULL OR v_location_ids IS NULL THEN
            RAISE NOTICE 'Missing products or locations';
            RETURN;
        END IF;

        -- Receipt movements (goods received from PO)
        FOR i IN 1..10 LOOP
            v_seq := v_seq + 1;
            INSERT INTO "StockMovements" (
                "Id", "TenantId", "MovementNumber", "MovementType",
                "ProductId", "Sku", "FromLocationId", "ToLocationId",
                "Quantity", "BatchNumber",
                "ReferenceType", "ReferenceNumber",
                "ReasonCode", "Notes", "MovementDate", "CreatedAt"
            ) VALUES (
                gen_random_uuid(), v_tenant_id,
                'MOV-2024-' || LPAD(v_seq::TEXT, 5, '0'),
                0, -- Receipt
                v_product_ids[(i % array_length(v_product_ids, 1)) + 1],
                v_product_skus[(i % array_length(v_product_skus, 1)) + 1],
                v_rcv_location_id,
                v_location_ids[(i % array_length(v_location_ids, 1)) + 1],
                (50 + (RANDOM() * 100))::INTEGER,
                'BATCH-2024-RCV-' || LPAD(i::TEXT, 3, '0'),
                'PurchaseOrder',
                'PO-2024-' || LPAD(i::TEXT, 4, '0'),
                NULL,
                'Received from supplier',
                NOW() - INTERVAL '30 days' + (i * INTERVAL '2 days'),
                NOW()
            );
        END LOOP;

        -- Issue movements (goods shipped out for SO)
        FOR i IN 1..8 LOOP
            v_seq := v_seq + 1;
            INSERT INTO "StockMovements" (
                "Id", "TenantId", "MovementNumber", "MovementType",
                "ProductId", "Sku", "FromLocationId", "ToLocationId",
                "Quantity", "BatchNumber",
                "ReferenceType", "ReferenceNumber",
                "Notes", "MovementDate", "CreatedAt"
            ) VALUES (
                gen_random_uuid(), v_tenant_id,
                'MOV-2024-' || LPAD(v_seq::TEXT, 5, '0'),
                1, -- Issue
                v_product_ids[(i % array_length(v_product_ids, 1)) + 1],
                v_product_skus[(i % array_length(v_product_skus, 1)) + 1],
                v_location_ids[(i % array_length(v_location_ids, 1)) + 1],
                NULL,
                (10 + (RANDOM() * 30))::INTEGER,
                NULL,
                'SalesOrder',
                'SO-2024-' || LPAD(i::TEXT, 4, '0'),
                'Shipped to customer',
                NOW() - INTERVAL '20 days' + (i * INTERVAL '2 days'),
                NOW()
            );
        END LOOP;

        -- Transfer movements (internal transfers)
        FOR i IN 1..5 LOOP
            v_seq := v_seq + 1;
            INSERT INTO "StockMovements" (
                "Id", "TenantId", "MovementNumber", "MovementType",
                "ProductId", "Sku", "FromLocationId", "ToLocationId",
                "Quantity",
                "ReferenceType", "ReferenceNumber",
                "Notes", "MovementDate", "CreatedAt"
            ) VALUES (
                gen_random_uuid(), v_tenant_id,
                'MOV-2024-' || LPAD(v_seq::TEXT, 5, '0'),
                2, -- Transfer
                v_product_ids[(i % array_length(v_product_ids, 1)) + 1],
                v_product_skus[(i % array_length(v_product_skus, 1)) + 1],
                v_location_ids[i],
                v_location_ids[(i + 5)],
                (20 + (RANDOM() * 30))::INTEGER,
                'StockTransfer',
                'TRF-2024-' || LPAD(i::TEXT, 4, '0'),
                'Replenishment from bulk to pick',
                NOW() - INTERVAL '10 days' + (i * INTERVAL '1 day'),
                NOW()
            );
        END LOOP;

        -- Adjustment movements
        FOR i IN 1..4 LOOP
            v_seq := v_seq + 1;
            INSERT INTO "StockMovements" (
                "Id", "TenantId", "MovementNumber", "MovementType",
                "ProductId", "Sku", "FromLocationId", "ToLocationId",
                "Quantity",
                "ReferenceType", "ReferenceNumber",
                "ReasonCode", "Notes", "MovementDate", "CreatedAt"
            ) VALUES (
                gen_random_uuid(), v_tenant_id,
                'MOV-2024-' || LPAD(v_seq::TEXT, 5, '0'),
                3, -- Adjustment
                v_product_ids[(i % array_length(v_product_ids, 1)) + 1],
                v_product_skus[(i % array_length(v_product_skus, 1)) + 1],
                CASE WHEN i % 2 = 0 THEN v_location_ids[i] ELSE NULL END, -- decrease
                CASE WHEN i % 2 = 1 THEN v_location_ids[i] ELSE NULL END, -- increase
                CASE WHEN i % 2 = 0 THEN -5 ELSE 5 END, -- positive or negative
                'Adjustment',
                'ADJ-2024-' || LPAD(i::TEXT, 4, '0'),
                CASE WHEN i % 2 = 0 THEN 'DAMAGED' ELSE 'FOUND' END,
                CASE WHEN i % 2 = 0 THEN 'Damaged goods written off' ELSE 'Extra items found during count' END,
                NOW() - INTERVAL '5 days' + (i * INTERVAL '1 day'),
                NOW()
            );
        END LOOP;

        -- Return movements
        FOR i IN 1..3 LOOP
            v_seq := v_seq + 1;
            INSERT INTO "StockMovements" (
                "Id", "TenantId", "MovementNumber", "MovementType",
                "ProductId", "Sku", "FromLocationId", "ToLocationId",
                "Quantity",
                "ReferenceType", "ReferenceNumber",
                "ReasonCode", "Notes", "MovementDate", "CreatedAt"
            ) VALUES (
                gen_random_uuid(), v_tenant_id,
                'MOV-2024-' || LPAD(v_seq::TEXT, 5, '0'),
                4, -- Return
                v_product_ids[i],
                v_product_skus[i],
                NULL, -- from external
                v_location_ids[i],
                (5 + (RANDOM() * 10))::INTEGER,
                'ReturnOrder',
                'RET-2024-' || LPAD(i::TEXT, 4, '0'),
                'CUSTOMER_RETURN',
                'Customer return - item in good condition',
                NOW() - INTERVAL '3 days' + (i * INTERVAL '1 day'),
                NOW()
            );
        END LOOP;

        -- Cycle count adjustment
        v_seq := v_seq + 1;
        INSERT INTO "StockMovements" (
            "Id", "TenantId", "MovementNumber", "MovementType",
            "ProductId", "Sku", "FromLocationId", "ToLocationId",
            "Quantity",
            "ReferenceType", "ReferenceNumber",
            "ReasonCode", "Notes", "MovementDate", "CreatedAt"
        ) VALUES (
            gen_random_uuid(), v_tenant_id,
            'MOV-2024-' || LPAD(v_seq::TEXT, 5, '0'),
            6, -- CycleCount
            v_product_ids[1],
            v_product_skus[1],
            v_location_ids[1],
            v_location_ids[1],
            2, -- variance found
            'CycleCount',
            'CC-2024-0001',
            'CYCLE_COUNT',
            'Variance found during cycle count - adjusted +2',
            NOW() - INTERVAL '1 day',
            NOW()
        );

        RAISE NOTICE 'Successfully inserted % stock movements', v_seq;
    ELSE
        RAISE NOTICE 'Stock movements already exist, skipping seed';
    END IF;
END $$;

-- MovementType enum reference:
-- 0 = Receipt, 1 = Issue, 2 = Transfer, 3 = Adjustment, 4 = Return, 5 = WriteOff, 6 = CycleCount

-- Useful queries:
-- SELECT * FROM "StockMovements" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46' ORDER BY "MovementDate" DESC;

-- Movement summary by type
-- SELECT "MovementType", COUNT(*), SUM("Quantity")
-- FROM "StockMovements"
-- WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46'
-- GROUP BY "MovementType";
