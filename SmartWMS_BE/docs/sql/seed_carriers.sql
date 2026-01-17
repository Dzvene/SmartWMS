-- ============================================
-- SmartWMS - Seed Carriers and Carrier Services Data
-- PostgreSQL
-- ============================================
-- TenantId: e9006ab8-257f-4021-b60a-cbba785bad46
-- ============================================
-- CarrierIntegrationType enum: 0=Manual, 1=Api, 2=Plugin, 3=Edi
-- ServiceType enum: 0=Ground, 1=Express, 2=NextDay, 3=SameDay, 4=Economy, 5=International, 6=Freight, 7=Parcel
-- ============================================

DO $$
DECLARE
    v_tenant_id UUID := 'e9006ab8-257f-4021-b60a-cbba785bad46';

    -- Carrier IDs
    v_dhl UUID := '33333333-0001-0001-0001-000000000001';
    v_fedex UUID := '33333333-0002-0001-0001-000000000001';
    v_ups UUID := '33333333-0003-0001-0001-000000000001';
    v_postnord UUID := '33333333-0004-0001-0001-000000000001';
    v_dbschenker UUID := '33333333-0005-0001-0001-000000000001';
    v_bring UUID := '33333333-0006-0001-0001-000000000001';
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "Carriers" WHERE "TenantId" = v_tenant_id LIMIT 1) THEN

        -- Insert Carriers
        INSERT INTO "Carriers" (
            "Id", "TenantId", "Code", "Name", "Description",
            "ContactName", "Phone", "Email", "Website",
            "AccountNumber", "IntegrationType",
            "ApiEndpoint", "ApiKey",
            "IsActive", "DefaultServiceCode", "Notes", "CreatedAt"
        ) VALUES
        (
            v_dhl, v_tenant_id, 'DHL', 'DHL Express', 'DHL Express worldwide shipping',
            'DHL Support', '+46 8 587 870 00', 'support.se@dhl.com', 'https://www.dhl.se',
            'SE-123456789', 1, -- API
            'https://api.dhl.com/express', 'dhl_api_key_placeholder',
            true, 'EXPRESS_WORLDWIDE', 'Primary international carrier', NOW()
        ),
        (
            v_fedex, v_tenant_id, 'FEDEX', 'FedEx', 'FedEx Express and Ground services',
            'FedEx Support', '+46 20 252 252', 'support@fedex.com', 'https://www.fedex.com/sv-se',
            'SE-987654321', 1, -- API
            'https://api.fedex.com', 'fedex_api_key_placeholder',
            true, 'FEDEX_EXPRESS', 'Secondary international carrier', NOW()
        ),
        (
            v_ups, v_tenant_id, 'UPS', 'UPS', 'United Parcel Service',
            'UPS Support', '+46 8 587 177 77', 'support.se@ups.com', 'https://www.ups.com/se',
            'SE-456789123', 1, -- API
            'https://onlinetools.ups.com/api', 'ups_api_key_placeholder',
            true, 'EXPRESS_SAVER', NULL, NOW()
        ),
        (
            v_postnord, v_tenant_id, 'POSTNORD', 'PostNord', 'Nordic postal and logistics service',
            'PostNord Kundservice', '+46 771 33 33 10', 'kundservice@postnord.se', 'https://www.postnord.se',
            'SE-789123456', 1, -- API
            'https://api2.postnord.com', 'postnord_api_key_placeholder',
            true, 'MYPACK_HOME', 'Primary Nordic carrier', NOW()
        ),
        (
            v_dbschenker, v_tenant_id, 'SCHENKER', 'DB Schenker', 'DB Schenker logistics',
            'Schenker Support', '+46 10 516 40 00', 'info.se@dbschenker.com', 'https://www.dbschenker.com/se-sv',
            'SE-321654987', 0, -- Manual
            NULL, NULL,
            true, 'PARCEL', 'Freight and pallet shipments', NOW()
        ),
        (
            v_bring, v_tenant_id, 'BRING', 'Bring', 'Norwegian/Nordic logistics',
            'Bring Support', '+47 22 03 00 00', 'kundeservice@bring.no', 'https://www.bring.no',
            'NO-111222333', 1, -- API
            'https://api.bring.com', 'bring_api_key_placeholder',
            true, 'PAKKE_I_POSTKASSEN', 'Norway shipments', NOW()
        ),
        -- Inactive carrier for testing
        (
            gen_random_uuid(), v_tenant_id, 'OLD_CARRIER', 'Old Carrier (Inactive)', 'Deactivated carrier',
            NULL, NULL, NULL, NULL,
            NULL, 0, -- Manual
            NULL, NULL,
            false, NULL, 'No longer in use', NOW()
        );

        -- Insert Carrier Services for DHL
        INSERT INTO "CarrierServices" (
            "Id", "TenantId", "CarrierId", "Code", "Name", "Description",
            "MinTransitDays", "MaxTransitDays", "ServiceType",
            "HasTracking", "TrackingUrlTemplate",
            "MaxWeightKg", "MaxLengthCm", "MaxWidthCm", "MaxHeightCm",
            "IsActive", "Notes", "CreatedAt"
        ) VALUES
        (gen_random_uuid(), v_tenant_id, v_dhl, 'EXPRESS_WORLDWIDE', 'DHL Express Worldwide', 'International express delivery',
         1, 3, 1, -- Express
         true, 'https://www.dhl.com/en/express/tracking.html?AWB={tracking_number}',
         70, 120, 80, 80, true, NULL, NOW()),
        (gen_random_uuid(), v_tenant_id, v_dhl, 'EXPRESS_9', 'DHL Express 9:00', 'Delivery before 9:00 AM',
         1, 1, 2, -- NextDay
         true, 'https://www.dhl.com/en/express/tracking.html?AWB={tracking_number}',
         30, 100, 60, 60, true, 'Premium service', NOW()),
        (gen_random_uuid(), v_tenant_id, v_dhl, 'EXPRESS_12', 'DHL Express 12:00', 'Delivery before 12:00 PM',
         1, 1, 2, -- NextDay
         true, 'https://www.dhl.com/en/express/tracking.html?AWB={tracking_number}',
         50, 120, 80, 80, true, NULL, NOW()),
        (gen_random_uuid(), v_tenant_id, v_dhl, 'ECONOMY_SELECT', 'DHL Economy Select', 'Economical road freight',
         3, 7, 0, -- Ground
         true, 'https://www.dhl.com/en/express/tracking.html?AWB={tracking_number}',
         500, 240, 200, 200, true, 'Pallet shipments', NOW());

        -- Insert Carrier Services for FedEx
        INSERT INTO "CarrierServices" (
            "Id", "TenantId", "CarrierId", "Code", "Name", "Description",
            "MinTransitDays", "MaxTransitDays", "ServiceType",
            "HasTracking", "TrackingUrlTemplate",
            "MaxWeightKg", "MaxLengthCm", "MaxWidthCm", "MaxHeightCm",
            "IsActive", "CreatedAt"
        ) VALUES
        (gen_random_uuid(), v_tenant_id, v_fedex, 'FEDEX_EXPRESS', 'FedEx International Priority', 'Express international delivery',
         1, 3, 1, true, 'https://www.fedex.com/fedextrack/?trknbr={tracking_number}',
         68, 270, 120, 120, true, NOW()),
        (gen_random_uuid(), v_tenant_id, v_fedex, 'FEDEX_ECONOMY', 'FedEx International Economy', 'Cost-effective international',
         4, 7, 4, true, 'https://www.fedex.com/fedextrack/?trknbr={tracking_number}',
         68, 270, 120, 120, true, NOW()),
        (gen_random_uuid(), v_tenant_id, v_fedex, 'FEDEX_GROUND', 'FedEx Ground', 'Ground delivery service',
         3, 5, 0, true, 'https://www.fedex.com/fedextrack/?trknbr={tracking_number}',
         68, 270, 120, 120, true, NOW());

        -- Insert Carrier Services for UPS
        INSERT INTO "CarrierServices" (
            "Id", "TenantId", "CarrierId", "Code", "Name", "Description",
            "MinTransitDays", "MaxTransitDays", "ServiceType",
            "HasTracking", "TrackingUrlTemplate",
            "MaxWeightKg", "MaxLengthCm", "MaxWidthCm", "MaxHeightCm",
            "IsActive", "CreatedAt"
        ) VALUES
        (gen_random_uuid(), v_tenant_id, v_ups, 'EXPRESS_SAVER', 'UPS Express Saver', 'Next business day delivery',
         1, 1, 2, true, 'https://www.ups.com/track?tracknum={tracking_number}',
         70, 270, 120, 120, true, NOW()),
        (gen_random_uuid(), v_tenant_id, v_ups, 'EXPRESS_PLUS', 'UPS Express Plus', 'Early morning delivery',
         1, 1, 2, true, 'https://www.ups.com/track?tracknum={tracking_number}',
         70, 270, 120, 120, true, NOW()),
        (gen_random_uuid(), v_tenant_id, v_ups, 'STANDARD', 'UPS Standard', 'Day-definite European delivery',
         2, 5, 0, true, 'https://www.ups.com/track?tracknum={tracking_number}',
         70, 270, 120, 120, true, NOW());

        -- Insert Carrier Services for PostNord
        INSERT INTO "CarrierServices" (
            "Id", "TenantId", "CarrierId", "Code", "Name", "Description",
            "MinTransitDays", "MaxTransitDays", "ServiceType",
            "HasTracking", "TrackingUrlTemplate",
            "MaxWeightKg", "MaxLengthCm", "MaxWidthCm", "MaxHeightCm",
            "IsActive", "CreatedAt"
        ) VALUES
        (gen_random_uuid(), v_tenant_id, v_postnord, 'MYPACK_HOME', 'MyPack Home', 'Home delivery with notification',
         1, 3, 7, true, 'https://www.postnord.se/tracking?id={tracking_number}',
         20, 100, 60, 60, true, NOW()),
        (gen_random_uuid(), v_tenant_id, v_postnord, 'MYPACK_COLLECT', 'MyPack Collect', 'Collect at service point',
         1, 3, 7, true, 'https://www.postnord.se/tracking?id={tracking_number}',
         20, 100, 60, 60, true, NOW()),
        (gen_random_uuid(), v_tenant_id, v_postnord, 'VARUBREV', 'Varubrev', 'Letter-sized parcels',
         2, 5, 4, true, 'https://www.postnord.se/tracking?id={tracking_number}',
         2, 60, 40, 3, true, NOW()),
        (gen_random_uuid(), v_tenant_id, v_postnord, 'PALLET', 'PostNord Pallet', 'Pallet freight service',
         1, 3, 6, true, 'https://www.postnord.se/tracking?id={tracking_number}',
         1000, 120, 80, 220, true, NOW());

        -- Insert Carrier Services for DB Schenker
        INSERT INTO "CarrierServices" (
            "Id", "TenantId", "CarrierId", "Code", "Name", "Description",
            "MinTransitDays", "MaxTransitDays", "ServiceType",
            "HasTracking", "TrackingUrlTemplate",
            "MaxWeightKg", "MaxLengthCm",
            "IsActive", "CreatedAt"
        ) VALUES
        (gen_random_uuid(), v_tenant_id, v_dbschenker, 'PARCEL', 'Schenker Parcel', 'Parcel delivery service',
         1, 3, 7, true, 'https://www.dbschenker.com/se-sv/tracking?id={tracking_number}',
         31.5, 240, true, NOW()),
        (gen_random_uuid(), v_tenant_id, v_dbschenker, 'SYSTEM', 'Schenker System', 'Pallet and groupage freight',
         1, 5, 6, true, 'https://www.dbschenker.com/se-sv/tracking?id={tracking_number}',
         2500, 240, true, NOW()),
        (gen_random_uuid(), v_tenant_id, v_dbschenker, 'DIRECT', 'Schenker Direct', 'Full truck load',
         1, 2, 6, true, 'https://www.dbschenker.com/se-sv/tracking?id={tracking_number}',
         24000, NULL, true, NOW());

        -- Insert Carrier Services for Bring
        INSERT INTO "CarrierServices" (
            "Id", "TenantId", "CarrierId", "Code", "Name", "Description",
            "MinTransitDays", "MaxTransitDays", "ServiceType",
            "HasTracking", "TrackingUrlTemplate",
            "MaxWeightKg", "MaxLengthCm", "MaxWidthCm", "MaxHeightCm",
            "IsActive", "CreatedAt"
        ) VALUES
        (gen_random_uuid(), v_tenant_id, v_bring, 'PAKKE_I_POSTKASSEN', 'Pakke i postkassen', 'Mailbox delivery',
         2, 5, 7, true, 'https://tracking.bring.com/tracking/{tracking_number}',
         2, 35, 25, 5, true, NOW()),
        (gen_random_uuid(), v_tenant_id, v_bring, 'BEDRIFTSPAKKE', 'Bedriftspakke', 'Business parcel delivery',
         1, 3, 0, true, 'https://tracking.bring.com/tracking/{tracking_number}',
         35, 150, 100, 100, true, NOW()),
        (gen_random_uuid(), v_tenant_id, v_bring, 'EXPRESS_NORDIC', 'Bring Express Nordic', 'Express next-day Nordic',
         1, 1, 2, true, 'https://tracking.bring.com/tracking/{tracking_number}',
         35, 150, 100, 100, true, NOW());

        RAISE NOTICE 'Successfully inserted 7 carriers and 21 carrier services';
    ELSE
        RAISE NOTICE 'Carriers already exist, skipping seed';
    END IF;
END $$;

-- CarrierIntegrationType enum reference:
-- 0 = Manual, 1 = Api, 2 = Plugin, 3 = Edi

-- ServiceType enum reference:
-- 0 = Ground, 1 = Express, 2 = NextDay, 3 = SameDay, 4 = Economy, 5 = International, 6 = Freight, 7 = Parcel

-- Useful queries:
-- SELECT * FROM "Carriers" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
-- SELECT c."Name", cs.* FROM "CarrierServices" cs JOIN "Carriers" c ON cs."CarrierId" = c."Id" WHERE cs."TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
