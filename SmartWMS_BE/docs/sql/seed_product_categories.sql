-- ============================================
-- SmartWMS - Seed Product Categories Data
-- PostgreSQL
-- ============================================
-- TenantId: e9006ab8-257f-4021-b60a-cbba785bad46
-- ============================================

DO $$
DECLARE
    v_tenant_id UUID := 'e9006ab8-257f-4021-b60a-cbba785bad46';

    -- Root category IDs
    v_cat_electronics UUID := '11111111-0001-0001-0001-000000000001';
    v_cat_clothing UUID := '11111111-0002-0001-0001-000000000001';
    v_cat_food UUID := '11111111-0003-0001-0001-000000000001';
    v_cat_office UUID := '11111111-0004-0001-0001-000000000001';
    v_cat_sports UUID := '11111111-0005-0001-0001-000000000001';
    v_cat_home UUID := '11111111-0006-0001-0001-000000000001';
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "ProductCategories" WHERE "TenantId" = v_tenant_id LIMIT 1) THEN

        -- Root Categories (Level 0)
        INSERT INTO "ProductCategories" (
            "Id", "TenantId", "Code", "Name", "Description",
            "ParentCategoryId", "Level", "Path",
            "IsActive", "DefaultUnitOfMeasure",
            "RequiresBatchTracking", "RequiresSerialTracking", "RequiresExpiryDate",
            "IsHazardous", "IsFragile", "CreatedAt"
        ) VALUES
        (v_cat_electronics, v_tenant_id, 'ELEC', 'Electronics', 'Electronic devices and components',
         NULL, 0, 'ELEC', true, 'EA', false, false, false, false, true, NOW()),

        (v_cat_clothing, v_tenant_id, 'CLOTH', 'Clothing & Apparel', 'Clothing, shoes, and accessories',
         NULL, 0, 'CLOTH', true, 'EA', false, false, false, false, false, NOW()),

        (v_cat_food, v_tenant_id, 'FOOD', 'Food & Beverages', 'Food products and drinks',
         NULL, 0, 'FOOD', true, 'EA', true, false, true, false, false, NOW()),

        (v_cat_office, v_tenant_id, 'OFFICE', 'Office Supplies', 'Office and stationery products',
         NULL, 0, 'OFFICE', true, 'EA', false, false, false, false, false, NOW()),

        (v_cat_sports, v_tenant_id, 'SPORTS', 'Sports & Outdoors', 'Sports equipment and outdoor gear',
         NULL, 0, 'SPORTS', true, 'EA', false, false, false, false, false, NOW()),

        (v_cat_home, v_tenant_id, 'HOME', 'Home & Garden', 'Home products and garden supplies',
         NULL, 0, 'HOME', true, 'EA', false, false, false, false, true, NOW());

        -- Electronics Subcategories (Level 1)
        INSERT INTO "ProductCategories" (
            "Id", "TenantId", "Code", "Name", "Description",
            "ParentCategoryId", "Level", "Path",
            "IsActive", "DefaultUnitOfMeasure",
            "RequiresSerialTracking", "IsFragile", "CreatedAt"
        ) VALUES
        ('11111111-0001-0002-0001-000000000001', v_tenant_id, 'ELEC-COMP', 'Computers', 'Laptops, desktops, tablets',
         v_cat_electronics, 1, 'ELEC/COMP', true, 'EA', true, true, NOW()),

        ('11111111-0001-0002-0002-000000000001', v_tenant_id, 'ELEC-PHONE', 'Mobile Phones', 'Smartphones and accessories',
         v_cat_electronics, 1, 'ELEC/PHONE', true, 'EA', true, true, NOW()),

        ('11111111-0001-0002-0003-000000000001', v_tenant_id, 'ELEC-AUDIO', 'Audio Equipment', 'Speakers, headphones, microphones',
         v_cat_electronics, 1, 'ELEC/AUDIO', true, 'EA', false, true, NOW()),

        ('11111111-0001-0002-0004-000000000001', v_tenant_id, 'ELEC-ACC', 'Electronics Accessories', 'Cables, adapters, chargers',
         v_cat_electronics, 1, 'ELEC/ACC', true, 'EA', false, false, NOW());

        -- Clothing Subcategories (Level 1)
        INSERT INTO "ProductCategories" (
            "Id", "TenantId", "Code", "Name", "Description",
            "ParentCategoryId", "Level", "Path",
            "IsActive", "DefaultUnitOfMeasure", "CreatedAt"
        ) VALUES
        ('11111111-0002-0002-0001-000000000001', v_tenant_id, 'CLOTH-MEN', 'Men''s Clothing', 'Men''s apparel',
         v_cat_clothing, 1, 'CLOTH/MEN', true, 'EA', NOW()),

        ('11111111-0002-0002-0002-000000000001', v_tenant_id, 'CLOTH-WOM', 'Women''s Clothing', 'Women''s apparel',
         v_cat_clothing, 1, 'CLOTH/WOM', true, 'EA', NOW()),

        ('11111111-0002-0002-0003-000000000001', v_tenant_id, 'CLOTH-KIDS', 'Kids'' Clothing', 'Children''s apparel',
         v_cat_clothing, 1, 'CLOTH/KIDS', true, 'EA', NOW()),

        ('11111111-0002-0002-0004-000000000001', v_tenant_id, 'CLOTH-SHOES', 'Footwear', 'Shoes, boots, sandals',
         v_cat_clothing, 1, 'CLOTH/SHOES', true, 'PAIR', NOW());

        -- Food Subcategories (Level 1)
        INSERT INTO "ProductCategories" (
            "Id", "TenantId", "Code", "Name", "Description",
            "ParentCategoryId", "Level", "Path",
            "IsActive", "DefaultUnitOfMeasure",
            "RequiresBatchTracking", "RequiresExpiryDate",
            "MinTemperature", "MaxTemperature", "CreatedAt"
        ) VALUES
        ('11111111-0003-0002-0001-000000000001', v_tenant_id, 'FOOD-FRESH', 'Fresh Products', 'Fresh meat, dairy, produce',
         v_cat_food, 1, 'FOOD/FRESH', true, 'EA', true, true, 2, 8, NOW()),

        ('11111111-0003-0002-0002-000000000001', v_tenant_id, 'FOOD-FROZEN', 'Frozen Foods', 'Frozen meals and ingredients',
         v_cat_food, 1, 'FOOD/FROZEN', true, 'EA', true, true, -25, -18, NOW()),

        ('11111111-0003-0002-0003-000000000001', v_tenant_id, 'FOOD-DRY', 'Dry Goods', 'Canned goods, cereals, pasta',
         v_cat_food, 1, 'FOOD/DRY', true, 'EA', true, true, NULL, NULL, NOW()),

        ('11111111-0003-0002-0004-000000000001', v_tenant_id, 'FOOD-BEV', 'Beverages', 'Drinks and beverages',
         v_cat_food, 1, 'FOOD/BEV', true, 'EA', true, true, NULL, NULL, NOW());

        -- Office Subcategories (Level 1)
        INSERT INTO "ProductCategories" (
            "Id", "TenantId", "Code", "Name", "Description",
            "ParentCategoryId", "Level", "Path",
            "IsActive", "DefaultUnitOfMeasure", "CreatedAt"
        ) VALUES
        ('11111111-0004-0002-0001-000000000001', v_tenant_id, 'OFFICE-PAPER', 'Paper Products', 'Paper, notebooks, printing supplies',
         v_cat_office, 1, 'OFFICE/PAPER', true, 'EA', NOW()),

        ('11111111-0004-0002-0002-000000000001', v_tenant_id, 'OFFICE-WRITE', 'Writing Instruments', 'Pens, pencils, markers',
         v_cat_office, 1, 'OFFICE/WRITE', true, 'EA', NOW()),

        ('11111111-0004-0002-0003-000000000001', v_tenant_id, 'OFFICE-FURN', 'Office Furniture', 'Desks, chairs, storage',
         v_cat_office, 1, 'OFFICE/FURN', true, 'EA', NOW());

        -- Sports Subcategories (Level 1)
        INSERT INTO "ProductCategories" (
            "Id", "TenantId", "Code", "Name", "Description",
            "ParentCategoryId", "Level", "Path",
            "IsActive", "DefaultUnitOfMeasure", "CreatedAt"
        ) VALUES
        ('11111111-0005-0002-0001-000000000001', v_tenant_id, 'SPORTS-FIT', 'Fitness Equipment', 'Gym and workout equipment',
         v_cat_sports, 1, 'SPORTS/FIT', true, 'EA', NOW()),

        ('11111111-0005-0002-0002-000000000001', v_tenant_id, 'SPORTS-OUT', 'Outdoor Gear', 'Camping, hiking equipment',
         v_cat_sports, 1, 'SPORTS/OUT', true, 'EA', NOW()),

        ('11111111-0005-0002-0003-000000000001', v_tenant_id, 'SPORTS-TEAM', 'Team Sports', 'Equipment for team sports',
         v_cat_sports, 1, 'SPORTS/TEAM', true, 'EA', NOW());

        -- Home Subcategories (Level 1)
        INSERT INTO "ProductCategories" (
            "Id", "TenantId", "Code", "Name", "Description",
            "ParentCategoryId", "Level", "Path",
            "IsActive", "DefaultUnitOfMeasure", "IsFragile", "CreatedAt"
        ) VALUES
        ('11111111-0006-0002-0001-000000000001', v_tenant_id, 'HOME-KITCH', 'Kitchen', 'Kitchen appliances and utensils',
         v_cat_home, 1, 'HOME/KITCH', true, 'EA', true, NOW()),

        ('11111111-0006-0002-0002-000000000001', v_tenant_id, 'HOME-DECOR', 'Home Decor', 'Decorative items',
         v_cat_home, 1, 'HOME/DECOR', true, 'EA', true, NOW()),

        ('11111111-0006-0002-0003-000000000001', v_tenant_id, 'HOME-GARDEN', 'Garden', 'Garden tools and plants',
         v_cat_home, 1, 'HOME/GARDEN', true, 'EA', false, NOW());

        RAISE NOTICE 'Successfully inserted 27 product categories (6 root + 21 subcategories)';
    ELSE
        RAISE NOTICE 'Product categories already exist, skipping seed';
    END IF;
END $$;

-- Useful queries:
-- SELECT * FROM "ProductCategories" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46' ORDER BY "Path";
-- SELECT * FROM "ProductCategories" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46' AND "Level" = 0;
