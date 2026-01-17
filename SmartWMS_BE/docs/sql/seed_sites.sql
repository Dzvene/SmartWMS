-- ============================================
-- SmartWMS - Seed Sites Data
-- PostgreSQL
-- ============================================
-- TenantId (CompanyId): e9006ab8-257f-4021-b60a-cbba785bad46
-- ============================================

DO $$
DECLARE
    v_tenant_id UUID := 'e9006ab8-257f-4021-b60a-cbba785bad46';
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "Sites" WHERE "TenantId" = v_tenant_id LIMIT 1) THEN

        INSERT INTO "Sites" (
            "Id", "TenantId", "CompanyId", "Code", "Name", "Description",
            "AddressLine1", "AddressLine2", "City", "Region", "PostalCode", "CountryCode",
            "Timezone", "IsActive", "IsPrimary", "CreatedAt"
        ) VALUES
        -- SITE001: Main Distribution Center (Stockholm)
        (
            'b1e2c3d4-5f6a-7b8c-9d0e-1f2a3b4c5d6e',
            v_tenant_id,
            v_tenant_id,
            'SITE001',
            'Stockholm Distribution Center',
            'Main distribution center for Nordic region',
            'Lagervägen 100',
            'Logistikparken',
            'Stockholm',
            'Stockholm',
            '104 35',
            'SE',
            'Europe/Stockholm',
            true,
            true,
            NOW()
        ),
        -- SITE002: Gothenburg Warehouse
        (
            'c2d3e4f5-6a7b-8c9d-0e1f-2a3b4c5d6e7f',
            v_tenant_id,
            v_tenant_id,
            'SITE002',
            'Gothenburg Warehouse',
            'West coast distribution hub',
            'Hamngatan 50',
            NULL,
            'Gothenburg',
            'Västra Götaland',
            '411 14',
            'SE',
            'Europe/Stockholm',
            true,
            false,
            NOW()
        ),
        -- SITE003: Oslo Branch
        (
            'd3e4f5a6-7b8c-9d0e-1f2a-3b4c5d6e7f8a',
            v_tenant_id,
            v_tenant_id,
            'SITE003',
            'Oslo Branch',
            'Norwegian operations center',
            'Industrigata 25',
            NULL,
            'Oslo',
            'Oslo',
            '0255',
            'NO',
            'Europe/Oslo',
            true,
            false,
            NOW()
        ),
        -- SITE004: Copenhagen Hub
        (
            'e4f5a6b7-8c9d-0e1f-2a3b-4c5d6e7f8a9b',
            v_tenant_id,
            v_tenant_id,
            'SITE004',
            'Copenhagen Hub',
            'Danish logistics hub',
            'Havnevej 15',
            'Building C',
            'Copenhagen',
            'Capital Region',
            '2100',
            'DK',
            'Europe/Copenhagen',
            true,
            false,
            NOW()
        ),
        -- SITE005: Inactive Site (for testing)
        (
            'f5a6b7c8-9d0e-1f2a-3b4c-5d6e7f8a9b0c',
            v_tenant_id,
            v_tenant_id,
            'SITE005',
            'Old Malmö Facility',
            'Decommissioned warehouse',
            'Gamla Vägen 5',
            NULL,
            'Malmö',
            'Skåne',
            '211 19',
            'SE',
            'Europe/Stockholm',
            false,
            false,
            NOW()
        );

        RAISE NOTICE 'Successfully inserted 5 sites';
    ELSE
        RAISE NOTICE 'Sites already exist, skipping seed';
    END IF;
END $$;

-- Useful queries:
-- SELECT * FROM "Sites" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
-- SELECT * FROM "Sites" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46' AND "IsActive" = true;
