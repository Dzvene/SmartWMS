-- ============================================
-- SmartWMS - Seed Customers Data
-- PostgreSQL
-- ============================================
-- TenantId: e9006ab8-257f-4021-b60a-cbba785bad46
--
-- This script inserts sample customer data for testing.
-- Run this if you need to manually seed customers without restarting the app.
-- ============================================

-- Check if customers already exist
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "Customers" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46' LIMIT 1) THEN

        INSERT INTO "Customers" (
            "Id", "TenantId", "Code", "Name", "ContactName", "Email", "Phone",
            "AddressLine1", "AddressLine2", "City", "Region", "PostalCode", "CountryCode",
            "TaxId", "PaymentTerms", "IsActive", "CreatedAt"
        ) VALUES
        -- CUST001: Acme Corporation (US)
        (
            gen_random_uuid(),
            'e9006ab8-257f-4021-b60a-cbba785bad46',
            'CUST001',
            'Acme Corporation',
            'John Smith',
            'john.smith@acme.com',
            '+1 555 123 4567',
            '123 Business Park',
            'Suite 100',
            'New York',
            'NY',
            '10001',
            'US',
            '12-3456789',
            'Net 30',
            true,
            NOW()
        ),
        -- CUST002: TechStart AB (Sweden)
        (
            gen_random_uuid(),
            'e9006ab8-257f-4021-b60a-cbba785bad46',
            'CUST002',
            'TechStart AB',
            'Erik Johansson',
            'erik@techstart.se',
            '+46 8 123 45 67',
            'Drottninggatan 50',
            NULL,
            'Stockholm',
            'Stockholm',
            '111 21',
            'SE',
            'SE556677889901',
            'Net 15',
            true,
            NOW()
        ),
        -- CUST003: Global Logistics GmbH (Germany)
        (
            gen_random_uuid(),
            'e9006ab8-257f-4021-b60a-cbba785bad46',
            'CUST003',
            'Global Logistics GmbH',
            'Hans Mueller',
            'h.mueller@globallogistics.de',
            '+49 30 987654',
            'Industriestrasse 42',
            NULL,
            'Berlin',
            'Berlin',
            '10115',
            'DE',
            'DE123456789',
            'Net 45',
            true,
            NOW()
        ),
        -- CUST004: Nordic Supplies (Norway)
        (
            gen_random_uuid(),
            'e9006ab8-257f-4021-b60a-cbba785bad46',
            'CUST004',
            'Nordic Supplies',
            'Anna Lindgren',
            'anna@nordicsupplies.no',
            '+47 22 33 44 55',
            'Storgata 15',
            NULL,
            'Oslo',
            'Oslo',
            '0154',
            'NO',
            'NO987654321MVA',
            'Net 30',
            true,
            NOW()
        ),
        -- CUST005: Pacific Trading Co (US - California)
        (
            gen_random_uuid(),
            'e9006ab8-257f-4021-b60a-cbba785bad46',
            'CUST005',
            'Pacific Trading Co',
            'Sarah Chen',
            's.chen@pacifictrading.com',
            '+1 415 555 0199',
            '888 Market Street',
            'Floor 12',
            'San Francisco',
            'CA',
            '94102',
            'US',
            '94-7654321',
            'Net 60',
            true,
            NOW()
        ),
        -- CUST006: EuroMart Distribution (France)
        (
            gen_random_uuid(),
            'e9006ab8-257f-4021-b60a-cbba785bad46',
            'CUST006',
            'EuroMart Distribution',
            'Pierre Dubois',
            'pdubois@euromart.fr',
            '+33 1 42 68 53 00',
            '25 Rue de la Paix',
            NULL,
            'Paris',
            'Île-de-France',
            '75002',
            'FR',
            'FR12345678901',
            'Net 30',
            true,
            NOW()
        ),
        -- CUST007: British Retail Ltd (UK)
        (
            gen_random_uuid(),
            'e9006ab8-257f-4021-b60a-cbba785bad46',
            'CUST007',
            'British Retail Ltd',
            'James Wilson',
            'j.wilson@britishretail.co.uk',
            '+44 20 7946 0958',
            '100 Oxford Street',
            NULL,
            'London',
            'Greater London',
            'W1D 1LL',
            'GB',
            'GB123456789',
            'Net 30',
            true,
            NOW()
        ),
        -- CUST008: Helsinki Electronics Oy (Finland)
        (
            gen_random_uuid(),
            'e9006ab8-257f-4021-b60a-cbba785bad46',
            'CUST008',
            'Helsinki Electronics Oy',
            'Mika Virtanen',
            'mika@helsinkielectronics.fi',
            '+358 9 123 4567',
            'Mannerheimintie 10',
            NULL,
            'Helsinki',
            'Uusimaa',
            '00100',
            'FI',
            'FI12345678',
            'Net 14',
            true,
            NOW()
        ),
        -- CUST009: Mediterranean Foods (Italy)
        (
            gen_random_uuid(),
            'e9006ab8-257f-4021-b60a-cbba785bad46',
            'CUST009',
            'Mediterranean Foods',
            'Marco Rossi',
            'm.rossi@medfoods.it',
            '+39 06 1234567',
            'Via Roma 25',
            NULL,
            'Rome',
            'Lazio',
            '00184',
            'IT',
            'IT12345678901',
            'Net 45',
            true,
            NOW()
        ),
        -- CUST010: Danish Design ApS (Denmark)
        (
            gen_random_uuid(),
            'e9006ab8-257f-4021-b60a-cbba785bad46',
            'CUST010',
            'Danish Design ApS',
            'Lars Nielsen',
            'lars@danishdesign.dk',
            '+45 33 12 34 56',
            'Strøget 55',
            NULL,
            'Copenhagen',
            'Capital Region',
            '1160',
            'DK',
            'DK12345678',
            'Net 30',
            true,
            NOW()
        ),
        -- CUST011: Inactive Corp (for testing inactive filter)
        (
            gen_random_uuid(),
            'e9006ab8-257f-4021-b60a-cbba785bad46',
            'CUST011',
            'Inactive Corp',
            'Old Contact',
            'old@inactive.com',
            '+1 555 000 0000',
            'Closed Business Park',
            NULL,
            'Nowhere',
            'NA',
            '00000',
            'US',
            NULL,
            'Net 30',
            false,
            NOW()
        ),
        -- CUST012: Warsaw Wholesale (Poland)
        (
            gen_random_uuid(),
            'e9006ab8-257f-4021-b60a-cbba785bad46',
            'CUST012',
            'Warsaw Wholesale Sp. z o.o.',
            'Piotr Kowalski',
            'p.kowalski@warsawwholesale.pl',
            '+48 22 123 45 67',
            'ul. Marszałkowska 100',
            NULL,
            'Warsaw',
            'Mazovia',
            '00-026',
            'PL',
            'PL1234567890',
            'Net 30',
            true,
            NOW()
        );

        RAISE NOTICE 'Successfully inserted 12 customers';
    ELSE
        RAISE NOTICE 'Customers already exist, skipping seed';
    END IF;
END $$;

-- ============================================
-- Useful Queries
-- ============================================

-- Get all customers
-- SELECT * FROM "Customers" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';

-- Get active customers only
-- SELECT * FROM "Customers" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46' AND "IsActive" = true;

-- Count customers by country
-- SELECT "CountryCode", COUNT(*) as customer_count
-- FROM "Customers"
-- WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46'
-- GROUP BY "CountryCode"
-- ORDER BY customer_count DESC;

-- Delete all customers (use with caution!)
-- DELETE FROM "Customers" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
