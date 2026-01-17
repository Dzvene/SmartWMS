-- ============================================
-- SmartWMS - Seed Equipment Data
-- PostgreSQL
-- ============================================
-- TenantId: e9006ab8-257f-4021-b60a-cbba785bad46
-- Main WarehouseId: ae5e6339-6212-4435-89a9-f5940815fe20
-- ============================================
-- EquipmentType enum: 0=Forklift, 1=PalletJack, 2=Scanner, 3=Printer, 4=Trolley
-- EquipmentStatus enum: 0=Available, 1=InUse, 2=Maintenance, 3=OutOfService
-- ============================================

DO $$
DECLARE
    v_tenant_id UUID := 'e9006ab8-257f-4021-b60a-cbba785bad46';
    v_wh_stockholm UUID := 'ae5e6339-6212-4435-89a9-f5940815fe20';
    v_zone_a UUID;
BEGIN
    -- Get actual zone from database
    SELECT "Id" INTO v_zone_a FROM "Zones" WHERE "TenantId" = v_tenant_id LIMIT 1;

    IF NOT EXISTS (SELECT 1 FROM "Equipment" WHERE "TenantId" = v_tenant_id LIMIT 1) THEN

        INSERT INTO "Equipment" (
            "Id", "TenantId", "Code", "Name", "Description",
            "Type", "Status", "WarehouseId", "ZoneId",
            "AssignedToUserId", "AssignedToUserName",
            "SerialNumber", "Manufacturer", "Model",
            "PurchaseDate", "WarrantyExpiryDate",
            "LastMaintenanceDate", "NextMaintenanceDate", "MaintenanceNotes",
            "Specifications", "IsActive", "CreatedAt"
        ) VALUES
        -- Forklifts
        (
            gen_random_uuid(), v_tenant_id, 'FLT-001', 'Forklift Alpha', 'Electric counterbalance forklift 2.5T',
            0, 0, -- Forklift, Available
            v_wh_stockholm, v_zone_a,
            NULL, NULL,
            'FLT-2021-A001', 'Toyota', 'Electric 2.5T',
            '2021-03-15', '2024-03-15',
            '2024-01-10', '2024-04-10', 'Regular 500h service completed',
            '{"capacity_kg": 2500, "lift_height_m": 6, "battery_type": "Li-Ion"}',
            true, NOW()
        ),
        (
            gen_random_uuid(), v_tenant_id, 'FLT-002', 'Forklift Bravo', 'Electric counterbalance forklift 2.5T',
            0, 1, -- Forklift, InUse
            v_wh_stockholm, v_zone_a,
            NULL, 'Erik Andersson',
            'FLT-2021-A002', 'Toyota', 'Electric 2.5T',
            '2021-03-15', '2024-03-15',
            '2023-12-05', '2024-03-05', NULL,
            '{"capacity_kg": 2500, "lift_height_m": 6, "battery_type": "Li-Ion"}',
            true, NOW()
        ),
        (
            gen_random_uuid(), v_tenant_id, 'FLT-003', 'Forklift Charlie', 'Reach truck for narrow aisles',
            0, 2, -- Forklift, Maintenance
            v_wh_stockholm, v_zone_a,
            NULL, NULL,
            'FLT-2022-R001', 'Jungheinrich', 'ETV 216i',
            '2022-06-20', '2025-06-20',
            '2024-01-15', '2024-01-20', 'Hydraulic system repair in progress',
            '{"capacity_kg": 1600, "lift_height_m": 10.5, "battery_type": "Li-Ion"}',
            true, NOW()
        ),
        (
            gen_random_uuid(), v_tenant_id, 'FLT-004', 'Forklift Delta', 'Heavy duty forklift 5T',
            0, 0, -- Forklift, Available
            v_wh_stockholm, NULL,
            NULL, NULL,
            'FLT-2020-H001', 'Linde', 'H50D',
            '2020-09-10', '2023-09-10',
            '2023-11-20', '2024-05-20', 'Diesel - 1000h service',
            '{"capacity_kg": 5000, "lift_height_m": 5, "fuel_type": "Diesel"}',
            true, NOW()
        ),

        -- Pallet Jacks
        (
            gen_random_uuid(), v_tenant_id, 'PJ-001', 'Pallet Jack 1', 'Electric pallet jack',
            1, 0, -- PalletJack, Available
            v_wh_stockholm, v_zone_a,
            NULL, NULL,
            'PJ-2022-E001', 'Crown', 'WP 3045',
            '2022-01-10', '2025-01-10',
            '2023-10-15', '2024-04-15', NULL,
            '{"capacity_kg": 2000, "fork_length_mm": 1150}',
            true, NOW()
        ),
        (
            gen_random_uuid(), v_tenant_id, 'PJ-002', 'Pallet Jack 2', 'Electric pallet jack',
            1, 1, -- PalletJack, InUse
            v_wh_stockholm, v_zone_a,
            NULL, 'Maria Svensson',
            'PJ-2022-E002', 'Crown', 'WP 3045',
            '2022-01-10', '2025-01-10',
            '2023-10-15', '2024-04-15', NULL,
            '{"capacity_kg": 2000, "fork_length_mm": 1150}',
            true, NOW()
        ),
        (
            gen_random_uuid(), v_tenant_id, 'PJ-003', 'Pallet Jack Manual', 'Manual pallet jack',
            1, 0, -- PalletJack, Available
            v_wh_stockholm, NULL,
            NULL, NULL,
            'PJ-2019-M001', 'Jungheinrich', 'AM 22',
            '2019-05-01', NULL,
            '2023-08-01', '2024-08-01', NULL,
            '{"capacity_kg": 2200, "fork_length_mm": 1150, "type": "manual"}',
            true, NOW()
        ),

        -- Scanners
        (
            gen_random_uuid(), v_tenant_id, 'SCN-001', 'Scanner 1', 'Handheld barcode scanner',
            2, 0, -- Scanner, Available
            v_wh_stockholm, NULL,
            NULL, NULL,
            'SCN-2023-H001', 'Zebra', 'MC9300',
            '2023-02-15', '2026-02-15',
            NULL, NULL, NULL,
            '{"type": "handheld", "connectivity": "WiFi+Bluetooth", "os": "Android"}',
            true, NOW()
        ),
        (
            gen_random_uuid(), v_tenant_id, 'SCN-002', 'Scanner 2', 'Handheld barcode scanner',
            2, 1, -- Scanner, InUse
            v_wh_stockholm, v_zone_a,
            NULL, 'Johan Lindqvist',
            'SCN-2023-H002', 'Zebra', 'MC9300',
            '2023-02-15', '2026-02-15',
            NULL, NULL, NULL,
            '{"type": "handheld", "connectivity": "WiFi+Bluetooth", "os": "Android"}',
            true, NOW()
        ),
        (
            gen_random_uuid(), v_tenant_id, 'SCN-003', 'Scanner 3', 'Handheld barcode scanner',
            2, 0, -- Scanner, Available
            v_wh_stockholm, NULL,
            NULL, NULL,
            'SCN-2023-H003', 'Zebra', 'MC9300',
            '2023-02-15', '2026-02-15',
            NULL, NULL, NULL,
            '{"type": "handheld", "connectivity": "WiFi+Bluetooth", "os": "Android"}',
            true, NOW()
        ),
        (
            gen_random_uuid(), v_tenant_id, 'SCN-004', 'Scanner Ring', 'Wearable ring scanner',
            2, 1, -- Scanner, InUse
            v_wh_stockholm, v_zone_a,
            NULL, 'Anna Berg',
            'SCN-2023-R001', 'Honeywell', '8675i',
            '2023-06-01', '2026-06-01',
            NULL, NULL, NULL,
            '{"type": "ring", "connectivity": "Bluetooth", "battery_life_hours": 12}',
            true, NOW()
        ),
        (
            gen_random_uuid(), v_tenant_id, 'SCN-005', 'Scanner Damaged', 'Broken scanner - pending repair',
            2, 3, -- Scanner, OutOfService
            v_wh_stockholm, NULL,
            NULL, NULL,
            'SCN-2022-H099', 'Zebra', 'MC9200',
            '2022-01-01', '2025-01-01',
            '2023-12-01', NULL, 'Screen cracked - sent for repair',
            '{"type": "handheld", "connectivity": "WiFi+Bluetooth"}',
            false, NOW()
        ),

        -- Printers
        (
            gen_random_uuid(), v_tenant_id, 'PRN-001', 'Label Printer 1', 'Shipping label printer',
            3, 0, -- Printer, Available
            v_wh_stockholm, v_zone_a,
            NULL, NULL,
            'PRN-2023-L001', 'Zebra', 'ZT411',
            '2023-01-20', '2026-01-20',
            NULL, NULL, NULL,
            '{"type": "thermal_transfer", "max_width_mm": 104, "dpi": 300}',
            true, NOW()
        ),
        (
            gen_random_uuid(), v_tenant_id, 'PRN-002', 'Label Printer 2', 'Shipping label printer',
            3, 1, -- Printer, InUse
            v_wh_stockholm, v_zone_a,
            NULL, NULL,
            'PRN-2023-L002', 'Zebra', 'ZT411',
            '2023-01-20', '2026-01-20',
            NULL, NULL, NULL,
            '{"type": "thermal_transfer", "max_width_mm": 104, "dpi": 300}',
            true, NOW()
        ),
        (
            gen_random_uuid(), v_tenant_id, 'PRN-003', 'Mobile Printer', 'Portable label printer',
            3, 0, -- Printer, Available
            v_wh_stockholm, NULL,
            NULL, NULL,
            'PRN-2023-M001', 'Zebra', 'ZQ630',
            '2023-03-10', '2026-03-10',
            NULL, NULL, NULL,
            '{"type": "mobile", "max_width_mm": 114, "connectivity": "WiFi+Bluetooth"}',
            true, NOW()
        ),

        -- Trolleys
        (
            gen_random_uuid(), v_tenant_id, 'TRL-001', 'Pick Cart 1', 'Multi-tier picking cart',
            4, 0, -- Trolley, Available
            v_wh_stockholm, v_zone_a,
            NULL, NULL,
            NULL, 'Custom', 'Pick Cart 3-Tier',
            '2021-08-01', NULL,
            NULL, NULL, NULL,
            '{"tiers": 3, "bins_per_tier": 6, "max_weight_kg": 200}',
            true, NOW()
        ),
        (
            gen_random_uuid(), v_tenant_id, 'TRL-002', 'Pick Cart 2', 'Multi-tier picking cart',
            4, 1, -- Trolley, InUse
            v_wh_stockholm, v_zone_a,
            NULL, 'Lisa Eriksson',
            NULL, 'Custom', 'Pick Cart 3-Tier',
            '2021-08-01', NULL,
            NULL, NULL, NULL,
            '{"tiers": 3, "bins_per_tier": 6, "max_weight_kg": 200}',
            true, NOW()
        ),
        (
            gen_random_uuid(), v_tenant_id, 'TRL-003', 'Cage Trolley', 'Roll cage container',
            4, 0, -- Trolley, Available
            v_wh_stockholm, NULL,
            NULL, NULL,
            NULL, 'Custom', 'Roll Cage 800x600',
            '2020-01-01', NULL,
            NULL, NULL, NULL,
            '{"dimensions_mm": "800x600x1800", "max_weight_kg": 500}',
            true, NOW()
        );

        RAISE NOTICE 'Successfully inserted 18 equipment items';
    ELSE
        RAISE NOTICE 'Equipment already exist, skipping seed';
    END IF;
END $$;

-- EquipmentType enum reference:
-- 0 = Forklift, 1 = PalletJack, 2 = Scanner, 3 = Printer, 4 = Trolley

-- EquipmentStatus enum reference:
-- 0 = Available, 1 = InUse, 2 = Maintenance, 3 = OutOfService

-- Useful queries:
-- SELECT * FROM "Equipment" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
-- SELECT * FROM "Equipment" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46' AND "Type" = 0; -- Forklifts
-- SELECT * FROM "Equipment" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46' AND "Status" = 0; -- Available
