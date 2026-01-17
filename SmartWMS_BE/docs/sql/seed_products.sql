-- ============================================
-- SmartWMS - Seed Products Data
-- PostgreSQL
-- ============================================
-- TenantId: e9006ab8-257f-4021-b60a-cbba785bad46
-- ============================================

DO $$
DECLARE
    v_tenant_id UUID := 'e9006ab8-257f-4021-b60a-cbba785bad46';

    -- Category IDs
    v_cat_comp UUID := '11111111-0001-0002-0001-000000000001';
    v_cat_phone UUID := '11111111-0001-0002-0002-000000000001';
    v_cat_audio UUID := '11111111-0001-0002-0003-000000000001';
    v_cat_elec_acc UUID := '11111111-0001-0002-0004-000000000001';
    v_cat_men UUID := '11111111-0002-0002-0001-000000000001';
    v_cat_women UUID := '11111111-0002-0002-0002-000000000001';
    v_cat_shoes UUID := '11111111-0002-0002-0004-000000000001';
    v_cat_fresh UUID := '11111111-0003-0002-0001-000000000001';
    v_cat_dry UUID := '11111111-0003-0002-0003-000000000001';
    v_cat_bev UUID := '11111111-0003-0002-0004-000000000001';
    v_cat_paper UUID := '11111111-0004-0002-0001-000000000001';
    v_cat_write UUID := '11111111-0004-0002-0002-000000000001';
    v_cat_fitness UUID := '11111111-0005-0002-0001-000000000001';
    v_cat_outdoor UUID := '11111111-0005-0002-0002-000000000001';
    v_cat_kitchen UUID := '11111111-0006-0002-0001-000000000001';
    v_cat_decor UUID := '11111111-0006-0002-0002-000000000001';
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "Products" WHERE "TenantId" = v_tenant_id LIMIT 1) THEN

        INSERT INTO "Products" (
            "Id", "TenantId", "Sku", "Name", "Description", "CategoryId",
            "Barcode", "WidthMm", "HeightMm", "DepthMm",
            "GrossWeightKg", "NetWeightKg", "UnitOfMeasure",
            "UnitsPerCase", "CasesPerPallet",
            "IsActive", "IsBatchTracked", "IsSerialTracked", "HasExpiryDate",
            "MinStockLevel", "MaxStockLevel", "ReorderPoint",
            "CreatedAt"
        ) VALUES
        -- Electronics - Computers
        ('22222222-0001-0001-0001-000000000001', v_tenant_id, 'LAPTOP-001', 'ProBook Laptop 15"', '15.6 inch laptop with Intel i7, 16GB RAM, 512GB SSD',
         v_cat_comp, '5901234567890', 360, 250, 20, 2.1, 2.0, 'EA', 1, 20,
         true, false, true, false, 10, 100, 20, NOW()),

        ('22222222-0001-0001-0002-000000000001', v_tenant_id, 'LAPTOP-002', 'UltraBook Air 13"', '13.3 inch ultralight laptop, 8GB RAM, 256GB SSD',
         v_cat_comp, '5901234567891', 310, 220, 15, 1.2, 1.1, 'EA', 1, 25,
         true, false, true, false, 15, 150, 30, NOW()),

        ('22222222-0001-0001-0003-000000000001', v_tenant_id, 'TABLET-001', 'TabPro 10.5"', '10.5 inch tablet with stylus support',
         v_cat_comp, '5901234567892', 250, 175, 8, 0.5, 0.48, 'EA', 2, 40,
         true, false, true, false, 20, 200, 40, NOW()),

        -- Electronics - Phones
        ('22222222-0001-0002-0001-000000000001', v_tenant_id, 'PHONE-001', 'SmartPhone Pro Max', 'Flagship smartphone 6.7 inch display',
         v_cat_phone, '5901234567893', 160, 78, 8, 0.23, 0.22, 'EA', 5, 100,
         true, false, true, false, 50, 500, 100, NOW()),

        ('22222222-0001-0002-0002-000000000001', v_tenant_id, 'PHONE-002', 'SmartPhone Lite', 'Budget-friendly smartphone 6.1 inch',
         v_cat_phone, '5901234567894', 150, 72, 8, 0.18, 0.17, 'EA', 6, 120,
         true, false, true, false, 80, 800, 150, NOW()),

        -- Electronics - Audio
        ('22222222-0001-0003-0001-000000000001', v_tenant_id, 'HEADPHONE-001', 'WirelessPro Headphones', 'Premium wireless noise-canceling headphones',
         v_cat_audio, '5901234567895', 200, 180, 80, 0.35, 0.32, 'EA', 4, 60,
         true, false, false, false, 30, 300, 60, NOW()),

        ('22222222-0001-0003-0002-000000000001', v_tenant_id, 'EARBUDS-001', 'TrueWireless Earbuds', 'Compact true wireless earbuds with case',
         v_cat_audio, '5901234567896', 60, 60, 30, 0.08, 0.06, 'EA', 10, 200,
         true, false, false, false, 100, 1000, 200, NOW()),

        ('22222222-0001-0003-0003-000000000001', v_tenant_id, 'SPEAKER-001', 'PortaSound Speaker', 'Portable Bluetooth speaker',
         v_cat_audio, '5901234567897', 180, 80, 80, 0.65, 0.6, 'EA', 4, 48,
         true, false, false, false, 25, 250, 50, NOW()),

        -- Electronics - Accessories
        ('22222222-0001-0004-0001-000000000001', v_tenant_id, 'CABLE-USB-C', 'USB-C Cable 1m', 'Premium braided USB-C cable',
         v_cat_elec_acc, '5901234567898', 150, 80, 20, 0.05, 0.04, 'EA', 20, 400,
         true, false, false, false, 200, 2000, 400, NOW()),

        ('22222222-0001-0004-0002-000000000001', v_tenant_id, 'CHARGER-65W', '65W Fast Charger', 'GaN fast charger USB-C',
         v_cat_elec_acc, '5901234567899', 65, 65, 30, 0.12, 0.1, 'EA', 10, 200,
         true, false, false, false, 100, 1000, 200, NOW()),

        -- Clothing - Men
        ('22222222-0002-0001-0001-000000000001', v_tenant_id, 'SHIRT-M-001', 'Men Classic Shirt Blue L', 'Cotton business shirt, size L',
         v_cat_men, '5902234567890', 400, 300, 30, 0.25, 0.22, 'EA', 10, 100,
         true, false, false, false, 20, 200, 40, NOW()),

        ('22222222-0002-0001-0002-000000000001', v_tenant_id, 'JEANS-M-001', 'Men Slim Jeans 32/32', 'Classic blue denim jeans 32x32',
         v_cat_men, '5902234567891', 400, 350, 40, 0.65, 0.6, 'EA', 6, 60,
         true, false, false, false, 15, 150, 30, NOW()),

        ('22222222-0002-0001-0003-000000000001', v_tenant_id, 'TSHIRT-M-001', 'Men Cotton T-Shirt Black M', 'Premium cotton t-shirt, size M',
         v_cat_men, '5902234567892', 350, 280, 20, 0.18, 0.15, 'EA', 12, 120,
         true, false, false, false, 50, 500, 100, NOW()),

        -- Clothing - Women
        ('22222222-0002-0002-0001-000000000001', v_tenant_id, 'DRESS-W-001', 'Women Summer Dress S', 'Floral print summer dress, size S',
         v_cat_women, '5902234567893', 400, 350, 25, 0.3, 0.28, 'EA', 8, 80,
         true, false, false, false, 15, 150, 30, NOW()),

        ('22222222-0002-0002-0002-000000000001', v_tenant_id, 'BLOUSE-W-001', 'Women Silk Blouse M', 'Elegant silk blouse, size M',
         v_cat_women, '5902234567894', 380, 300, 20, 0.2, 0.18, 'EA', 10, 100,
         true, false, false, false, 12, 120, 25, NOW()),

        -- Clothing - Shoes
        ('22222222-0002-0004-0001-000000000001', v_tenant_id, 'SNEAKER-001', 'Running Sneakers Size 42', 'Lightweight running shoes EU 42',
         v_cat_shoes, '5902234567895', 320, 120, 200, 0.75, 0.7, 'PAIR', 6, 48,
         true, false, false, false, 20, 200, 40, NOW()),

        ('22222222-0002-0004-0002-000000000001', v_tenant_id, 'BOOTS-001', 'Leather Boots Size 40', 'Classic leather boots EU 40',
         v_cat_shoes, '5902234567896', 320, 150, 280, 1.2, 1.1, 'PAIR', 4, 32,
         true, false, false, false, 10, 100, 20, NOW()),

        -- Food - Fresh (with batch and expiry tracking)
        ('22222222-0003-0001-0001-000000000001', v_tenant_id, 'CHEESE-001', 'Aged Cheddar 500g', 'Premium aged cheddar cheese',
         v_cat_fresh, '5903234567890', 150, 100, 60, 0.55, 0.5, 'EA', 10, 80,
         true, true, false, true, 30, 300, 60, NOW()),

        ('22222222-0003-0001-0002-000000000001', v_tenant_id, 'YOGURT-001', 'Greek Yogurt 200g', 'Natural Greek yogurt',
         v_cat_fresh, '5903234567891', 80, 80, 100, 0.22, 0.2, 'EA', 24, 96,
         true, true, false, true, 100, 1000, 200, NOW()),

        -- Food - Dry Goods (with batch and expiry tracking)
        ('22222222-0003-0003-0001-000000000001', v_tenant_id, 'PASTA-001', 'Italian Pasta 500g', 'Premium durum wheat pasta',
         v_cat_dry, '5903234567892', 250, 150, 50, 0.52, 0.5, 'EA', 20, 160,
         true, true, false, true, 50, 500, 100, NOW()),

        ('22222222-0003-0003-0002-000000000001', v_tenant_id, 'RICE-001', 'Basmati Rice 1kg', 'Premium basmati rice',
         v_cat_dry, '5903234567893', 300, 200, 80, 1.05, 1.0, 'EA', 12, 96,
         true, true, false, true, 40, 400, 80, NOW()),

        ('22222222-0003-0003-0003-000000000001', v_tenant_id, 'CEREAL-001', 'Organic Muesli 750g', 'Premium organic muesli blend',
         v_cat_dry, '5903234567894', 280, 180, 100, 0.8, 0.75, 'EA', 8, 64,
         true, true, false, true, 30, 300, 60, NOW()),

        -- Food - Beverages
        ('22222222-0003-0004-0001-000000000001', v_tenant_id, 'COFFEE-001', 'Premium Coffee Beans 1kg', 'Single origin arabica beans',
         v_cat_bev, '5903234567895', 200, 300, 100, 1.1, 1.0, 'EA', 6, 48,
         true, true, false, true, 25, 250, 50, NOW()),

        ('22222222-0003-0004-0002-000000000001', v_tenant_id, 'TEA-001', 'English Breakfast Tea 100 bags', 'Classic English breakfast tea',
         v_cat_bev, '5903234567896', 150, 100, 80, 0.25, 0.2, 'EA', 12, 144,
         true, true, false, true, 50, 500, 100, NOW()),

        -- Office - Paper
        ('22222222-0004-0001-0001-000000000001', v_tenant_id, 'PAPER-A4', 'Copy Paper A4 500 sheets', 'Premium white copy paper 80gsm',
         v_cat_paper, '5904234567890', 297, 210, 50, 2.5, 2.4, 'REAM', 5, 50,
         true, false, false, false, 100, 1000, 200, NOW()),

        ('22222222-0004-0001-0002-000000000001', v_tenant_id, 'NOTEBOOK-001', 'Spiral Notebook A5', 'Ruled spiral notebook 100 pages',
         v_cat_paper, '5904234567891', 210, 148, 15, 0.2, 0.18, 'EA', 20, 200,
         true, false, false, false, 80, 800, 160, NOW()),

        -- Office - Writing
        ('22222222-0004-0002-0001-000000000001', v_tenant_id, 'PEN-BALL-001', 'Ballpoint Pen Black 12pk', 'Medium point ballpoint pens',
         v_cat_write, '5904234567892', 160, 140, 20, 0.15, 0.12, 'PKG', 24, 288,
         true, false, false, false, 100, 1000, 200, NOW()),

        ('22222222-0004-0002-0002-000000000001', v_tenant_id, 'MARKER-001', 'Highlighter Set 6 colors', 'Fluorescent highlighter markers',
         v_cat_write, '5904234567893', 180, 100, 25, 0.12, 0.1, 'SET', 20, 240,
         true, false, false, false, 60, 600, 120, NOW()),

        -- Sports - Fitness
        ('22222222-0005-0001-0001-000000000001', v_tenant_id, 'DUMBBELL-5KG', 'Dumbbell Set 5kg Pair', 'Neoprene coated dumbbells',
         v_cat_fitness, '5905234567890', 280, 120, 120, 10.5, 10.0, 'PAIR', 2, 16,
         true, false, false, false, 10, 100, 20, NOW()),

        ('22222222-0005-0001-0002-000000000001', v_tenant_id, 'YOGAMAT-001', 'Yoga Mat Premium', 'Non-slip yoga mat 6mm thick',
         v_cat_fitness, '5905234567891', 1800, 150, 60, 1.5, 1.4, 'EA', 6, 36,
         true, false, false, false, 15, 150, 30, NOW()),

        -- Sports - Outdoor
        ('22222222-0005-0002-0001-000000000001', v_tenant_id, 'TENT-2P', '2-Person Tent', 'Lightweight camping tent',
         v_cat_outdoor, '5905234567892', 450, 180, 180, 3.5, 3.2, 'EA', 2, 12,
         true, false, false, false, 8, 80, 15, NOW()),

        ('22222222-0005-0002-0002-000000000001', v_tenant_id, 'SLEEPING-001', 'Sleeping Bag -5C', 'Mummy sleeping bag rated to -5C',
         v_cat_outdoor, '5905234567893', 400, 250, 250, 2.0, 1.8, 'EA', 4, 24,
         true, false, false, false, 12, 120, 24, NOW()),

        -- Home - Kitchen
        ('22222222-0006-0001-0001-000000000001', v_tenant_id, 'BLENDER-001', 'Power Blender 1000W', 'High-speed blender with glass jar',
         v_cat_kitchen, '5906234567890', 220, 450, 200, 4.5, 4.2, 'EA', 2, 16,
         true, false, false, false, 10, 100, 20, NOW()),

        ('22222222-0006-0001-0002-000000000001', v_tenant_id, 'KETTLE-001', 'Electric Kettle 1.7L', 'Stainless steel electric kettle',
         v_cat_kitchen, '5906234567891', 220, 250, 160, 1.2, 1.0, 'EA', 4, 32,
         true, false, false, false, 20, 200, 40, NOW()),

        ('22222222-0006-0001-0003-000000000001', v_tenant_id, 'COOKSET-001', 'Cookware Set 10pc', 'Non-stick cookware set',
         v_cat_kitchen, '5906234567892', 500, 300, 400, 8.0, 7.5, 'SET', 1, 8,
         true, false, false, false, 5, 50, 10, NOW()),

        -- Home - Decor
        ('22222222-0006-0002-0001-000000000001', v_tenant_id, 'VASE-001', 'Glass Vase 30cm', 'Clear glass decorative vase',
         v_cat_decor, '5906234567893', 150, 300, 150, 1.2, 1.0, 'EA', 4, 32,
         true, false, false, false, 15, 150, 30, NOW()),

        ('22222222-0006-0002-0002-000000000001', v_tenant_id, 'FRAME-001', 'Photo Frame 20x30cm', 'Wooden photo frame',
         v_cat_decor, '5906234567894', 250, 350, 25, 0.5, 0.45, 'EA', 6, 72,
         true, false, false, false, 25, 250, 50, NOW()),

        -- Inactive product (for testing)
        ('22222222-9999-9999-9999-000000000001', v_tenant_id, 'DISCONTINUED-001', 'Old Product (Inactive)', 'Discontinued product for testing',
         v_cat_kitchen, '5909999999999', 100, 100, 100, 0.5, 0.45, 'EA', 10, 100,
         false, false, false, false, 0, 0, 0, NOW());

        RAISE NOTICE 'Successfully inserted 40 products';
    ELSE
        RAISE NOTICE 'Products already exist, skipping seed';
    END IF;
END $$;

-- Useful queries:
-- SELECT * FROM "Products" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
-- SELECT p.*, c."Name" as CategoryName FROM "Products" p JOIN "ProductCategories" c ON p."CategoryId" = c."Id" WHERE p."TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
-- SELECT * FROM "Products" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46' AND "IsActive" = true ORDER BY "Sku";
