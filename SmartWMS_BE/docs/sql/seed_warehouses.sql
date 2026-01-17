-- ============================================
-- SmartWMS - Seed Warehouses Data
-- PostgreSQL
-- ============================================
-- TenantId: e9006ab8-257f-4021-b60a-cbba785bad46
-- Main WarehouseId: ae5e6339-6212-4435-89a9-f5940815fe20
-- ============================================

DO $$
DECLARE
    v_tenant_id UUID := 'e9006ab8-257f-4021-b60a-cbba785bad46';
    v_site_stockholm UUID := 'b1e2c3d4-5f6a-7b8c-9d0e-1f2a3b4c5d6e';
    v_site_gothenburg UUID := 'c2d3e4f5-6a7b-8c9d-0e1f-2a3b4c5d6e7f';
    v_site_oslo UUID := 'd3e4f5a6-7b8c-9d0e-1f2a-3b4c5d6e7f8a';
    v_site_copenhagen UUID := 'e4f5a6b7-8c9d-0e1f-2a3b-4c5d6e7f8a9b';
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "Warehouses" WHERE "TenantId" = v_tenant_id LIMIT 1) THEN

        INSERT INTO "Warehouses" (
            "Id", "TenantId", "SiteId", "Code", "Name", "Description",
            "AddressLine1", "AddressLine2", "City", "Region", "PostalCode", "CountryCode",
            "Timezone", "IsPrimary", "IsActive", "CreatedAt"
        ) VALUES
        -- WH001: Main Warehouse Stockholm (Primary)
        (
            'ae5e6339-6212-4435-89a9-f5940815fe20',
            v_tenant_id,
            v_site_stockholm,
            'WH001',
            'Stockholm Main',
            'Primary distribution warehouse - 50,000 sqm',
            'Lagervägen 100',
            'Building A',
            'Stockholm',
            'Stockholm',
            '104 35',
            'SE',
            'Europe/Stockholm',
            true,
            true,
            NOW()
        ),
        -- WH002: Stockholm Cold Storage
        (
            'be6f7440-7323-5546-8ab0-a6051926af31',
            v_tenant_id,
            v_site_stockholm,
            'WH002',
            'Stockholm Cold Storage',
            'Temperature-controlled warehouse for perishables',
            'Lagervägen 100',
            'Building B - Cold Zone',
            'Stockholm',
            'Stockholm',
            '104 35',
            'SE',
            'Europe/Stockholm',
            false,
            true,
            NOW()
        ),
        -- WH003: Gothenburg Warehouse
        (
            'cf708551-8434-6657-9bc1-b7162037ba42',
            v_tenant_id,
            v_site_gothenburg,
            'WH003',
            'Gothenburg West',
            'West coast distribution center - 25,000 sqm',
            'Hamngatan 50',
            NULL,
            'Gothenburg',
            'Västra Götaland',
            '411 14',
            'SE',
            'Europe/Stockholm',
            false,
            true,
            NOW()
        ),
        -- WH004: Oslo Warehouse
        (
            'da819662-9545-7768-0cd2-c8273148cb53',
            v_tenant_id,
            v_site_oslo,
            'WH004',
            'Oslo Central',
            'Norwegian distribution center - 15,000 sqm',
            'Industrigata 25',
            NULL,
            'Oslo',
            'Oslo',
            '0255',
            'NO',
            'Europe/Oslo',
            false,
            true,
            NOW()
        ),
        -- WH005: Copenhagen Cross-dock
        (
            'eb920773-0656-8879-1de3-d9384259de64',
            v_tenant_id,
            v_site_copenhagen,
            'WH005',
            'Copenhagen Cross-dock',
            'High-throughput cross-docking facility',
            'Havnevej 15',
            'Building C',
            'Copenhagen',
            'Capital Region',
            '2100',
            'DK',
            'Europe/Copenhagen',
            false,
            true,
            NOW()
        ),
        -- WH006: Inactive warehouse (for testing)
        (
            'fc031884-1767-9980-2ef4-e0495360ef75',
            v_tenant_id,
            v_site_stockholm,
            'WH006',
            'Old Stockholm Annex',
            'Decommissioned storage facility',
            'Lagervägen 100',
            'Building D - Closed',
            'Stockholm',
            'Stockholm',
            '104 35',
            'SE',
            'Europe/Stockholm',
            false,
            false,
            NOW()
        );

        RAISE NOTICE 'Successfully inserted 6 warehouses';
    ELSE
        RAISE NOTICE 'Warehouses already exist, skipping seed';
    END IF;
END $$;

-- Useful queries:
-- SELECT * FROM "Warehouses" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
-- SELECT w.*, s."Name" as SiteName FROM "Warehouses" w JOIN "Sites" s ON w."SiteId" = s."Id" WHERE w."TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
