-- ============================================
-- SmartWMS - Seed Locations Data
-- PostgreSQL
-- ============================================
-- TenantId: e9006ab8-257f-4021-b60a-cbba785bad46
-- Main WarehouseId: ae5e6339-6212-4435-89a9-f5940815fe20
-- ============================================
-- LocationType enum: 0=Bulk, 1=Pick, 2=Staging, 3=Receiving, 4=Shipping, 5=Returns, 6=Quarantine, 7=Reserve
-- ============================================

DO $$
DECLARE
    v_tenant_id UUID := 'e9006ab8-257f-4021-b60a-cbba785bad46';
    v_wh_stockholm UUID := 'ae5e6339-6212-4435-89a9-f5940815fe20';

    -- Stockholm Zone IDs
    v_zone_rcv UUID := 'a1111111-1111-1111-1111-111111111111';
    v_zone_stg_in UUID := 'a2222222-2222-2222-2222-222222222222';
    v_zone_blk_a UUID := 'a3333333-3333-3333-3333-333333333333';
    v_zone_blk_b UUID := 'a4444444-4444-4444-4444-444444444444';
    v_zone_pck_a UUID := 'a5555555-5555-5555-5555-555555555555';
    v_zone_pck_b UUID := 'a6666666-6666-6666-6666-666666666666';
    v_zone_pak UUID := 'a7777777-7777-7777-7777-777777777777';
    v_zone_stg_out UUID := 'a8888888-8888-8888-8888-888888888888';
    v_zone_shp UUID := 'a9999999-9999-9999-9999-999999999999';
    v_zone_ret UUID := 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';
    v_zone_qrn UUID := 'abababab-abab-abab-abab-abababababab';

    v_aisle TEXT;
    v_rack TEXT;
    v_level TEXT;
    v_seq INTEGER := 0;
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "Locations" WHERE "TenantId" = v_tenant_id LIMIT 1) THEN

        -- Receiving Dock Locations (DOCK-01 to DOCK-10)
        FOR i IN 1..10 LOOP
            v_seq := v_seq + 1;
            INSERT INTO "Locations" (
                "Id", "TenantId", "WarehouseId", "ZoneId", "Code", "Name",
                "Aisle", "Rack", "Level", "Position",
                "LocationType", "IsActive", "IsPickLocation", "IsPutawayLocation",
                "IsReceivingDock", "IsShippingDock", "PickSequence", "PutawaySequence",
                "WidthMm", "HeightMm", "DepthMm", "MaxWeight", "CreatedAt"
            ) VALUES (
                gen_random_uuid(), v_tenant_id, v_wh_stockholm, v_zone_rcv,
                'DOCK-RCV-' || LPAD(i::TEXT, 2, '0'),
                'Receiving Dock ' || i,
                'DOCK', 'RCV', '00', LPAD(i::TEXT, 2, '0'),
                3, -- Receiving
                true, false, false, true, false, NULL, NULL,
                3000, 4000, 15000, 20000,
                NOW()
            );
        END LOOP;

        -- Inbound Staging Locations (STG-IN-01 to STG-IN-20)
        FOR i IN 1..20 LOOP
            v_seq := v_seq + 1;
            INSERT INTO "Locations" (
                "Id", "TenantId", "WarehouseId", "ZoneId", "Code", "Name",
                "Aisle", "Rack", "Level", "Position",
                "LocationType", "IsActive", "IsPickLocation", "IsPutawayLocation",
                "PickSequence", "PutawaySequence", "MaxWeight", "CreatedAt"
            ) VALUES (
                gen_random_uuid(), v_tenant_id, v_wh_stockholm, v_zone_stg_in,
                'STG-IN-' || LPAD(i::TEXT, 2, '0'),
                'Inbound Staging ' || i,
                'STG', 'IN', '00', LPAD(i::TEXT, 2, '0'),
                2, -- Staging
                true, false, true, NULL, v_seq,
                5000,
                NOW()
            );
        END LOOP;

        -- Bulk Storage A - Aisles A01-A05, Racks 01-10, Levels 1-4
        FOR a IN 1..5 LOOP
            v_aisle := 'A' || LPAD(a::TEXT, 2, '0');
            FOR r IN 1..10 LOOP
                v_rack := LPAD(r::TEXT, 2, '0');
                FOR l IN 1..4 LOOP
                    v_level := LPAD(l::TEXT, 2, '0');
                    v_seq := v_seq + 1;
                    INSERT INTO "Locations" (
                        "Id", "TenantId", "WarehouseId", "ZoneId", "Code", "Name",
                        "Aisle", "Rack", "Level", "Position",
                        "LocationType", "IsActive", "IsPickLocation", "IsPutawayLocation",
                        "PickSequence", "PutawaySequence",
                        "WidthMm", "HeightMm", "DepthMm", "MaxWeight", "CreatedAt"
                    ) VALUES (
                        gen_random_uuid(), v_tenant_id, v_wh_stockholm, v_zone_blk_a,
                        v_aisle || '-' || v_rack || '-' || v_level,
                        'Bulk A ' || v_aisle || '-' || v_rack || '-' || v_level,
                        v_aisle, v_rack, v_level, '01',
                        0, -- Bulk
                        true, false, true, NULL, v_seq,
                        1200, 1800, 1200, 1500,
                        NOW()
                    );
                END LOOP;
            END LOOP;
        END LOOP;

        -- Bulk Storage B - Aisles B01-B05, Racks 01-10, Levels 1-4
        FOR a IN 1..5 LOOP
            v_aisle := 'B' || LPAD(a::TEXT, 2, '0');
            FOR r IN 1..10 LOOP
                v_rack := LPAD(r::TEXT, 2, '0');
                FOR l IN 1..4 LOOP
                    v_level := LPAD(l::TEXT, 2, '0');
                    v_seq := v_seq + 1;
                    INSERT INTO "Locations" (
                        "Id", "TenantId", "WarehouseId", "ZoneId", "Code", "Name",
                        "Aisle", "Rack", "Level", "Position",
                        "LocationType", "IsActive", "IsPickLocation", "IsPutawayLocation",
                        "PickSequence", "PutawaySequence",
                        "WidthMm", "HeightMm", "DepthMm", "MaxWeight", "CreatedAt"
                    ) VALUES (
                        gen_random_uuid(), v_tenant_id, v_wh_stockholm, v_zone_blk_b,
                        v_aisle || '-' || v_rack || '-' || v_level,
                        'Bulk B ' || v_aisle || '-' || v_rack || '-' || v_level,
                        v_aisle, v_rack, v_level, '01',
                        0, -- Bulk
                        true, false, true, NULL, v_seq,
                        1200, 1800, 1200, 1500,
                        NOW()
                    );
                END LOOP;
            END LOOP;
        END LOOP;

        -- Pick Zone A - Aisles PA01-PA03, Racks 01-20, Ground level only
        FOR a IN 1..3 LOOP
            v_aisle := 'PA' || LPAD(a::TEXT, 2, '0');
            FOR r IN 1..20 LOOP
                v_rack := LPAD(r::TEXT, 2, '0');
                v_seq := v_seq + 1;
                INSERT INTO "Locations" (
                    "Id", "TenantId", "WarehouseId", "ZoneId", "Code", "Name",
                    "Aisle", "Rack", "Level", "Position",
                    "LocationType", "IsActive", "IsPickLocation", "IsPutawayLocation",
                    "PickSequence", "PutawaySequence",
                    "WidthMm", "HeightMm", "DepthMm", "MaxWeight", "CreatedAt"
                ) VALUES (
                    gen_random_uuid(), v_tenant_id, v_wh_stockholm, v_zone_pck_a,
                    v_aisle || '-' || v_rack || '-01',
                    'Pick A ' || v_aisle || '-' || v_rack,
                    v_aisle, v_rack, '01', '01',
                    1, -- Pick
                    true, true, true, v_seq, v_seq,
                    600, 400, 800, 50,
                    NOW()
                );
            END LOOP;
        END LOOP;

        -- Pick Zone B - Aisles PB01-PB02, Racks 01-15
        FOR a IN 1..2 LOOP
            v_aisle := 'PB' || LPAD(a::TEXT, 2, '0');
            FOR r IN 1..15 LOOP
                v_rack := LPAD(r::TEXT, 2, '0');
                v_seq := v_seq + 1;
                INSERT INTO "Locations" (
                    "Id", "TenantId", "WarehouseId", "ZoneId", "Code", "Name",
                    "Aisle", "Rack", "Level", "Position",
                    "LocationType", "IsActive", "IsPickLocation", "IsPutawayLocation",
                    "PickSequence", "PutawaySequence",
                    "WidthMm", "HeightMm", "DepthMm", "MaxWeight", "CreatedAt"
                ) VALUES (
                    gen_random_uuid(), v_tenant_id, v_wh_stockholm, v_zone_pck_b,
                    v_aisle || '-' || v_rack || '-01',
                    'Pick B ' || v_aisle || '-' || v_rack,
                    v_aisle, v_rack, '01', '01',
                    1, -- Pick
                    true, true, true, v_seq, v_seq,
                    600, 400, 800, 50,
                    NOW()
                );
            END LOOP;
        END LOOP;

        -- Packing Stations (PACK-01 to PACK-20)
        FOR i IN 1..20 LOOP
            INSERT INTO "Locations" (
                "Id", "TenantId", "WarehouseId", "ZoneId", "Code", "Name",
                "Aisle", "Rack", "Level", "Position",
                "LocationType", "IsActive", "IsPickLocation", "IsPutawayLocation",
                "CreatedAt"
            ) VALUES (
                gen_random_uuid(), v_tenant_id, v_wh_stockholm, v_zone_pak,
                'PACK-' || LPAD(i::TEXT, 2, '0'),
                'Packing Station ' || i,
                'PACK', 'ST', '01', LPAD(i::TEXT, 2, '0'),
                2, -- Staging (packing area)
                true, false, false,
                NOW()
            );
        END LOOP;

        -- Outbound Staging (STG-OUT-01 to STG-OUT-30)
        FOR i IN 1..30 LOOP
            INSERT INTO "Locations" (
                "Id", "TenantId", "WarehouseId", "ZoneId", "Code", "Name",
                "Aisle", "Rack", "Level", "Position",
                "LocationType", "IsActive", "IsPickLocation", "IsPutawayLocation",
                "MaxWeight", "CreatedAt"
            ) VALUES (
                gen_random_uuid(), v_tenant_id, v_wh_stockholm, v_zone_stg_out,
                'STG-OUT-' || LPAD(i::TEXT, 2, '0'),
                'Outbound Staging ' || i,
                'STG', 'OUT', '00', LPAD(i::TEXT, 2, '0'),
                2, -- Staging
                true, false, false,
                5000,
                NOW()
            );
        END LOOP;

        -- Shipping Dock Locations (DOCK-SHP-01 to DOCK-SHP-08)
        FOR i IN 1..8 LOOP
            INSERT INTO "Locations" (
                "Id", "TenantId", "WarehouseId", "ZoneId", "Code", "Name",
                "Aisle", "Rack", "Level", "Position",
                "LocationType", "IsActive", "IsPickLocation", "IsPutawayLocation",
                "IsReceivingDock", "IsShippingDock",
                "WidthMm", "HeightMm", "DepthMm", "MaxWeight", "CreatedAt"
            ) VALUES (
                gen_random_uuid(), v_tenant_id, v_wh_stockholm, v_zone_shp,
                'DOCK-SHP-' || LPAD(i::TEXT, 2, '0'),
                'Shipping Dock ' || i,
                'DOCK', 'SHP', '00', LPAD(i::TEXT, 2, '0'),
                4, -- Shipping
                true, false, false, false, true,
                3000, 4000, 15000, 20000,
                NOW()
            );
        END LOOP;

        -- Returns Processing Locations (RET-01 to RET-10)
        FOR i IN 1..10 LOOP
            INSERT INTO "Locations" (
                "Id", "TenantId", "WarehouseId", "ZoneId", "Code", "Name",
                "Aisle", "Rack", "Level", "Position",
                "LocationType", "IsActive", "IsPickLocation", "IsPutawayLocation",
                "MaxWeight", "CreatedAt"
            ) VALUES (
                gen_random_uuid(), v_tenant_id, v_wh_stockholm, v_zone_ret,
                'RET-' || LPAD(i::TEXT, 2, '0'),
                'Returns Bin ' || i,
                'RET', 'PROC', '01', LPAD(i::TEXT, 2, '0'),
                5, -- Returns
                true, false, true,
                500,
                NOW()
            );
        END LOOP;

        -- Quarantine Locations (QRN-01 to QRN-05)
        FOR i IN 1..5 LOOP
            INSERT INTO "Locations" (
                "Id", "TenantId", "WarehouseId", "ZoneId", "Code", "Name",
                "Aisle", "Rack", "Level", "Position",
                "LocationType", "IsActive", "IsPickLocation", "IsPutawayLocation",
                "MaxWeight", "CreatedAt"
            ) VALUES (
                gen_random_uuid(), v_tenant_id, v_wh_stockholm, v_zone_qrn,
                'QRN-' || LPAD(i::TEXT, 2, '0'),
                'Quarantine Bay ' || i,
                'QRN', 'BAY', '01', LPAD(i::TEXT, 2, '0'),
                6, -- Quarantine
                true, false, true,
                2000,
                NOW()
            );
        END LOOP;

        RAISE NOTICE 'Successfully inserted locations: % receiving, % staging, % bulk, % pick, % pack, % shipping, % returns, % quarantine',
            10, 50, 400, 90, 20, 8, 10, 5;
    ELSE
        RAISE NOTICE 'Locations already exist, skipping seed';
    END IF;
END $$;

-- LocationType enum reference:
-- 0 = Bulk, 1 = Pick, 2 = Staging, 3 = Receiving, 4 = Shipping, 5 = Returns, 6 = Quarantine, 7 = Reserve

-- Useful queries:
-- SELECT COUNT(*) FROM "Locations" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
-- SELECT z."Name" as Zone, COUNT(*) as LocationCount FROM "Locations" l JOIN "Zones" z ON l."ZoneId" = z."Id" WHERE l."TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46' GROUP BY z."Name";
-- SELECT * FROM "Locations" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46' AND "IsPickLocation" = true;
