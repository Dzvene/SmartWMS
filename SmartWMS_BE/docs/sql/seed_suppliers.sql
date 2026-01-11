-- ============================================
-- SmartWMS - Seed Suppliers Data
-- PostgreSQL
-- ============================================
-- TenantId: e9006ab8-257f-4021-b60a-cbba785bad46
-- ============================================

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "Suppliers" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46' LIMIT 1) THEN

        INSERT INTO "Suppliers" (
            "Id", "TenantId", "Code", "Name", "ContactName", "Email", "Phone",
            "AddressLine1", "City", "Region", "PostalCode", "CountryCode",
            "TaxId", "PaymentTerms", "LeadTimeDays", "IsActive", "CreatedAt"
        ) VALUES
        (gen_random_uuid(), 'e9006ab8-257f-4021-b60a-cbba785bad46', 'SUPP001', 'TechSupply Inc', 'Michael Johnson', 'm.johnson@techsupply.com', '+1 555 987 6543', '500 Industrial Blvd', 'Chicago', 'IL', '60601', 'US', '36-1234567', 'Net 30', 7, true, NOW()),
        (gen_random_uuid(), 'e9006ab8-257f-4021-b60a-cbba785bad46', 'SUPP002', 'Nordic Components AB', 'Anna Svensson', 'anna@nordiccomp.se', '+46 8 555 1234', 'Industrivägen 25', 'Gothenburg', 'Västra Götaland', '411 04', 'SE', 'SE556677889902', 'Net 45', 14, true, NOW()),
        (gen_random_uuid(), 'e9006ab8-257f-4021-b60a-cbba785bad46', 'SUPP003', 'Deutsche Elektronik GmbH', 'Klaus Weber', 'k.weber@deutschelektronik.de', '+49 89 12345678', 'Münchner Straße 100', 'Munich', 'Bavaria', '80331', 'DE', 'DE987654321', 'Net 30', 10, true, NOW()),
        (gen_random_uuid(), 'e9006ab8-257f-4021-b60a-cbba785bad46', 'SUPP004', 'Asian Manufacturing Ltd', 'Li Wei', 'l.wei@asianmfg.cn', '+86 21 5555 8888', '888 Pudong Avenue', 'Shanghai', 'Shanghai', '200120', 'CN', 'CN912345678901234567', 'Net 60', 30, true, NOW()),
        (gen_random_uuid(), 'e9006ab8-257f-4021-b60a-cbba785bad46', 'SUPP005', 'UK Industrial Supplies', 'David Brown', 'd.brown@ukindustrial.co.uk', '+44 121 555 7890', '50 Birmingham Road', 'Birmingham', 'West Midlands', 'B1 1AA', 'GB', 'GB987654321', 'Net 30', 5, true, NOW()),
        (gen_random_uuid(), 'e9006ab8-257f-4021-b60a-cbba785bad46', 'SUPP006', 'French Parts SARL', 'Marie Dupont', 'm.dupont@frenchparts.fr', '+33 4 91 55 66 77', '15 Avenue de Marseille', 'Lyon', 'Auvergne-Rhône-Alpes', '69001', 'FR', 'FR98765432101', 'Net 45', 12, true, NOW()),
        (gen_random_uuid(), 'e9006ab8-257f-4021-b60a-cbba785bad46', 'SUPP007', 'Italian Quality SpA', 'Giuseppe Romano', 'g.romano@italianquality.it', '+39 02 555 1234', 'Via Milano 50', 'Milan', 'Lombardy', '20121', 'IT', 'IT12345678901', 'Net 30', 8, true, NOW()),
        (gen_random_uuid(), 'e9006ab8-257f-4021-b60a-cbba785bad46', 'SUPP008', 'Polish Manufacturing Sp. z o.o.', 'Jan Nowak', 'j.nowak@polishmfg.pl', '+48 22 555 6789', 'ul. Przemysłowa 200', 'Lodz', 'Lodzkie', '90-001', 'PL', 'PL9876543210', 'Net 30', 7, true, NOW()),
        (gen_random_uuid(), 'e9006ab8-257f-4021-b60a-cbba785bad46', 'SUPP009', 'Inactive Supplier Co', 'Old Contact', 'old@inactive-supplier.com', '+1 555 000 0001', 'Closed Factory', 'Nowhere', 'NA', '00001', 'US', NULL, 'Net 30', 0, false, NOW()),
        (gen_random_uuid(), 'e9006ab8-257f-4021-b60a-cbba785bad46', 'SUPP010', 'Japan Electronics Co Ltd', 'Takeshi Yamamoto', 't.yamamoto@japanelec.jp', '+81 3 5555 6666', '1-2-3 Akihabara', 'Tokyo', 'Tokyo', '101-0021', 'JP', 'JP1234567890123', 'Net 60', 21, true, NOW());

        RAISE NOTICE 'Successfully inserted 10 suppliers';
    ELSE
        RAISE NOTICE 'Suppliers already exist, skipping seed';
    END IF;
END $$;

-- Useful queries:
-- SELECT * FROM "Suppliers" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
