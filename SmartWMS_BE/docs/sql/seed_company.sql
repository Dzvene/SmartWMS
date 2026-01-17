-- ============================================
-- SmartWMS - Seed Company (Tenant) Data
-- PostgreSQL
-- ============================================
-- This is the PRIMARY seed file - must run FIRST
-- TenantId: e9006ab8-257f-4021-b60a-cbba785bad46
-- ============================================

DO $$
DECLARE
    v_tenant_id UUID := 'e9006ab8-257f-4021-b60a-cbba785bad46';
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "Companies" WHERE "Id" = v_tenant_id) THEN
        INSERT INTO "Companies" (
            "Id", "Name", "Code", "Description",
            "LegalName", "TaxId", "RegistrationNumber",
            "AddressLine1", "AddressLine2", "City", "Region", "PostalCode", "Country",
            "Phone", "Email", "Website",
            "LogoUrl", "PrimaryColor", "SecondaryColor",
            "DefaultCurrency", "DefaultTimezone", "DefaultLanguage",
            "MaxUsers", "MaxWarehouses", "MaxProducts",
            "SubscriptionPlan", "SubscriptionStatus", "SubscriptionExpiresAt",
            "IsActive", "CreatedAt"
        ) VALUES (
            v_tenant_id,
            'SmartWMS Demo Company',
            'DEMO',
            'Demonstration tenant for SmartWMS WMS system',
            'SmartWMS Demo AB',
            'SE556123456701',
            '556123-4567',
            'Kungsgatan 1',
            'Floor 3',
            'Stockholm',
            'Stockholm',
            '111 43',
            'Sweden',
            '+46 8 123 45 67',
            'info@smartwms-demo.com',
            'https://smartwms-demo.com',
            '/assets/logos/demo-logo.png',
            '#1976D2',
            '#424242',
            'SEK',
            'Europe/Stockholm',
            'en',
            50,
            10,
            10000,
            'Enterprise',
            'Active',
            NOW() + INTERVAL '365 days',
            true,
            NOW()
        );

        RAISE NOTICE 'Successfully inserted demo company (tenant)';
    ELSE
        RAISE NOTICE 'Company already exists, skipping seed';
    END IF;
END $$;

-- Useful queries:
-- SELECT * FROM "Companies" WHERE "Id" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
