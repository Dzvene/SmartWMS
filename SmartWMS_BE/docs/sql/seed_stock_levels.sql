-- ============================================
-- SmartWMS - Seed Stock Levels Data
-- PostgreSQL
-- ============================================
-- TenantId: e9006ab8-257f-4021-b60a-cbba785bad46
-- ============================================
-- NOTE: This script requires Locations and Products to exist first.
-- Run seed_locations.sql and seed_products.sql before this script.
-- ============================================

DO $$
DECLARE
    v_tenant_id UUID := 'e9006ab8-257f-4021-b60a-cbba785bad46';
    v_location_ids UUID[];
    v_product_ids UUID[];
    v_product_skus TEXT[];
    v_batch_tracked BOOLEAN[];
    v_has_expiry BOOLEAN[];
    v_idx INTEGER;
    v_loc_idx INTEGER;
    v_qty DECIMAL;
    v_reserved DECIMAL;
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "StockLevels" WHERE "TenantId" = v_tenant_id LIMIT 1) THEN

        -- Get location IDs (first 30)
        SELECT ARRAY_AGG("Id" ORDER BY "Code")
        INTO v_location_ids
        FROM (SELECT "Id", "Code" FROM "Locations"
              WHERE "TenantId" = v_tenant_id
              ORDER BY "Code" LIMIT 30) l;

        -- Get product info (first 30 active products)
        SELECT ARRAY_AGG("Id" ORDER BY "Sku"),
               ARRAY_AGG("Sku" ORDER BY "Sku"),
               ARRAY_AGG("IsBatchTracked" ORDER BY "Sku"),
               ARRAY_AGG("HasExpiryDate" ORDER BY "Sku")
        INTO v_product_ids, v_product_skus, v_batch_tracked, v_has_expiry
        FROM (SELECT "Id", "Sku", "IsBatchTracked", "HasExpiryDate"
              FROM "Products"
              WHERE "TenantId" = v_tenant_id AND "IsActive" = true
              ORDER BY "Sku" LIMIT 30) p;

        IF v_location_ids IS NULL OR v_product_ids IS NULL THEN
            RAISE NOTICE 'Missing locations or products, cannot seed stock levels';
            RETURN;
        END IF;

        -- Create stock levels for each product in a pick location
        FOR v_idx IN 1..array_length(v_product_ids, 1) LOOP
            v_loc_idx := ((v_idx - 1) % array_length(v_location_ids, 1)) + 1;
            v_qty := (50 + (RANDOM() * 150))::INTEGER;
            v_reserved := (RANDOM() * 10)::INTEGER;

            INSERT INTO "StockLevels" (
                "Id", "TenantId", "ProductId", "Sku", "LocationId",
                "QuantityOnHand", "QuantityReserved",
                "BatchNumber", "ExpiryDate",
                "LastMovementAt", "LastCountAt", "CreatedAt"
            ) VALUES (
                gen_random_uuid(), v_tenant_id,
                v_product_ids[v_idx], v_product_skus[v_idx], v_location_ids[v_loc_idx],
                v_qty, v_reserved,
                CASE WHEN v_batch_tracked[v_idx] THEN 'BATCH-2024-' || LPAD(v_idx::TEXT, 3, '0') ELSE NULL END,
                CASE WHEN v_has_expiry[v_idx] THEN NOW() + INTERVAL '180 days' ELSE NULL END,
                NOW() - INTERVAL '2 days',
                NOW() - INTERVAL '30 days',
                NOW()
            );

            -- Add a second batch for batch-tracked items with different expiry
            IF v_batch_tracked[v_idx] THEN
                INSERT INTO "StockLevels" (
                    "Id", "TenantId", "ProductId", "Sku", "LocationId",
                    "QuantityOnHand", "QuantityReserved",
                    "BatchNumber", "ExpiryDate",
                    "LastMovementAt", "CreatedAt"
                ) VALUES (
                    gen_random_uuid(), v_tenant_id,
                    v_product_ids[v_idx], v_product_skus[v_idx], v_location_ids[v_loc_idx],
                    (20 + (RANDOM() * 50))::INTEGER, 0,
                    'BATCH-2024-' || LPAD((v_idx + 100)::TEXT, 3, '0'),
                    NOW() + INTERVAL '90 days',
                    NOW() - INTERVAL '5 days',
                    NOW()
                );
            END IF;
        END LOOP;

        -- Note: Bulk storage stock seed removed for simplicity
        -- Only using available locations for stock

        RAISE NOTICE 'Successfully inserted stock levels';
    ELSE
        RAISE NOTICE 'Stock levels already exist, skipping seed';
    END IF;
END $$;

-- Useful queries:
-- SELECT * FROM "StockLevels" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
-- SELECT sl.*, p."Name" as ProductName, l."Code" as LocationCode
-- FROM "StockLevels" sl
-- JOIN "Products" p ON sl."ProductId" = p."Id"
-- JOIN "Locations" l ON sl."LocationId" = l."Id"
-- WHERE sl."TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';

-- Get total stock by product
-- SELECT "Sku", SUM("QuantityOnHand") as TotalOnHand, SUM("QuantityAvailable") as TotalAvailable
-- FROM "StockLevels"
-- WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46'
-- GROUP BY "Sku"
-- ORDER BY TotalOnHand DESC;
