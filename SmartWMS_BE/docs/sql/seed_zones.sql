-- ============================================
-- SmartWMS - Seed Zones Data
-- PostgreSQL
-- ============================================
-- TenantId: e9006ab8-257f-4021-b60a-cbba785bad46
-- Main WarehouseId: ae5e6339-6212-4435-89a9-f5940815fe20
-- ============================================
-- ZoneType enum: 0=Storage, 1=Picking, 2=Packing, 3=Staging, 4=Shipping, 5=Receiving, 6=Returns
-- ============================================

DO $$
DECLARE
    v_tenant_id UUID := 'e9006ab8-257f-4021-b60a-cbba785bad46';
    v_wh_stockholm UUID := 'ae5e6339-6212-4435-89a9-f5940815fe20';
    v_wh_cold UUID := 'be6f7440-7323-5546-8ab0-a6051926af31';
    v_wh_gothenburg UUID := 'cf708551-8434-6657-9bc1-b7162037ba42';
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "Zones" WHERE "TenantId" = v_tenant_id LIMIT 1) THEN

        INSERT INTO "Zones" (
            "Id", "TenantId", "WarehouseId", "Code", "Name", "Description",
            "ZoneType", "IsActive", "PickSequence", "CreatedAt"
        ) VALUES
        -- Stockholm Main Warehouse Zones
        (
            'a1111111-1111-1111-1111-111111111111',
            v_tenant_id,
            v_wh_stockholm,
            'STK-RCV',
            'Receiving Dock',
            'Inbound goods receiving area with 10 dock doors',
            5, -- Receiving
            true,
            NULL,
            NOW()
        ),
        (
            'a2222222-2222-2222-2222-222222222222',
            v_tenant_id,
            v_wh_stockholm,
            'STK-STG-IN',
            'Inbound Staging',
            'Staging area for received goods pending putaway',
            3, -- Staging
            true,
            NULL,
            NOW()
        ),
        (
            'a3333333-3333-3333-3333-333333333333',
            v_tenant_id,
            v_wh_stockholm,
            'STK-BLK-A',
            'Bulk Storage A',
            'High-rack bulk storage zone - Aisles A01-A20',
            0, -- Storage
            true,
            10,
            NOW()
        ),
        (
            'a4444444-4444-4444-4444-444444444444',
            v_tenant_id,
            v_wh_stockholm,
            'STK-BLK-B',
            'Bulk Storage B',
            'High-rack bulk storage zone - Aisles B01-B20',
            0, -- Storage
            true,
            20,
            NOW()
        ),
        (
            'a5555555-5555-5555-5555-555555555555',
            v_tenant_id,
            v_wh_stockholm,
            'STK-PCK-A',
            'Pick Zone A',
            'Forward pick zone for fast-moving items',
            1, -- Picking
            true,
            30,
            NOW()
        ),
        (
            'a6666666-6666-6666-6666-666666666666',
            v_tenant_id,
            v_wh_stockholm,
            'STK-PCK-B',
            'Pick Zone B',
            'Forward pick zone for medium-velocity items',
            1, -- Picking
            true,
            40,
            NOW()
        ),
        (
            'a7777777-7777-7777-7777-777777777777',
            v_tenant_id,
            v_wh_stockholm,
            'STK-PAK',
            'Packing Area',
            'Packing stations with 20 workstations',
            2, -- Packing
            true,
            50,
            NOW()
        ),
        (
            'a8888888-8888-8888-8888-888888888888',
            v_tenant_id,
            v_wh_stockholm,
            'STK-STG-OUT',
            'Outbound Staging',
            'Staging area for packed orders awaiting shipment',
            3, -- Staging
            true,
            60,
            NOW()
        ),
        (
            'a9999999-9999-9999-9999-999999999999',
            v_tenant_id,
            v_wh_stockholm,
            'STK-SHP',
            'Shipping Dock',
            'Outbound shipping area with 8 dock doors',
            4, -- Shipping
            true,
            70,
            NOW()
        ),
        (
            'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
            v_tenant_id,
            v_wh_stockholm,
            'STK-RET',
            'Returns Processing',
            'Returns inspection and processing area',
            6, -- Returns
            true,
            80,
            NOW()
        ),
        (
            'abababab-abab-abab-abab-abababababab',
            v_tenant_id,
            v_wh_stockholm,
            'STK-QRN',
            'Quarantine Zone',
            'Quality hold and damaged goods area',
            0, -- Storage (quarantine type)
            true,
            90,
            NOW()
        ),

        -- Cold Storage Warehouse Zones
        (
            'b1111111-1111-1111-1111-111111111111',
            v_tenant_id,
            v_wh_cold,
            'CLD-RCV',
            'Cold Receiving',
            'Temperature-controlled receiving dock',
            5, -- Receiving
            true,
            NULL,
            NOW()
        ),
        (
            'b2222222-2222-2222-2222-222222222222',
            v_tenant_id,
            v_wh_cold,
            'CLD-FRZ',
            'Freezer Storage',
            'Deep freeze storage -18C',
            0, -- Storage
            true,
            10,
            NOW()
        ),
        (
            'b3333333-3333-3333-3333-333333333333',
            v_tenant_id,
            v_wh_cold,
            'CLD-CHL',
            'Chilled Storage',
            'Refrigerated storage 2-8C',
            0, -- Storage
            true,
            20,
            NOW()
        ),
        (
            'b4444444-4444-4444-4444-444444444444',
            v_tenant_id,
            v_wh_cold,
            'CLD-SHP',
            'Cold Shipping',
            'Temperature-controlled shipping dock',
            4, -- Shipping
            true,
            30,
            NOW()
        ),

        -- Gothenburg Warehouse Zones
        (
            'c1111111-1111-1111-1111-111111111111',
            v_tenant_id,
            v_wh_gothenburg,
            'GBG-RCV',
            'GBG Receiving',
            'Gothenburg receiving dock',
            5, -- Receiving
            true,
            NULL,
            NOW()
        ),
        (
            'c2222222-2222-2222-2222-222222222222',
            v_tenant_id,
            v_wh_gothenburg,
            'GBG-STR',
            'GBG Storage',
            'Main storage area',
            0, -- Storage
            true,
            10,
            NOW()
        ),
        (
            'c3333333-3333-3333-3333-333333333333',
            v_tenant_id,
            v_wh_gothenburg,
            'GBG-PCK',
            'GBG Picking',
            'Picking zone',
            1, -- Picking
            true,
            20,
            NOW()
        ),
        (
            'c4444444-4444-4444-4444-444444444444',
            v_tenant_id,
            v_wh_gothenburg,
            'GBG-SHP',
            'GBG Shipping',
            'Gothenburg shipping dock',
            4, -- Shipping
            true,
            30,
            NOW()
        );

        RAISE NOTICE 'Successfully inserted 19 zones';
    ELSE
        RAISE NOTICE 'Zones already exist, skipping seed';
    END IF;
END $$;

-- ZoneType enum reference:
-- 0 = Storage, 1 = Picking, 2 = Packing, 3 = Staging, 4 = Shipping, 5 = Receiving, 6 = Returns

-- Useful queries:
-- SELECT * FROM "Zones" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
-- SELECT z.*, w."Name" as WarehouseName FROM "Zones" z JOIN "Warehouses" w ON z."WarehouseId" = w."Id" WHERE z."TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
