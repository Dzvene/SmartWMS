-- ============================================
-- SmartWMS - Seed Users & Roles Data
-- PostgreSQL
-- ============================================
-- Must run AFTER seed_company.sql
-- TenantId: e9006ab8-257f-4021-b60a-cbba785bad46
-- ============================================
-- NOTE: Passwords are hashed using ASP.NET Identity hasher
-- Default password for all users: "Demo123!"
-- In production, users should change passwords on first login
-- ============================================

DO $$
DECLARE
    v_tenant_id UUID := 'e9006ab8-257f-4021-b60a-cbba785bad46';
    v_role_admin UUID := 'a0000001-0001-0001-0001-000000000001';
    v_role_manager UUID := 'a0000001-0001-0001-0002-000000000001';
    v_role_supervisor UUID := 'a0000001-0001-0001-0003-000000000001';
    v_role_operator UUID := 'a0000001-0001-0001-0004-000000000001';
    v_role_picker UUID := 'a0000001-0001-0001-0005-000000000001';
    v_role_packer UUID := 'a0000001-0001-0001-0006-000000000001';
    v_role_receiver UUID := 'a0000001-0001-0001-0007-000000000001';
    v_role_viewer UUID := 'a0000001-0001-0001-0008-000000000001';
    v_user_admin UUID := 'b0000001-0001-0001-0001-000000000001';
    v_user_manager UUID := 'b0000001-0001-0001-0002-000000000001';
    v_user_supervisor UUID := 'b0000001-0001-0001-0003-000000000001';
    v_user_picker1 UUID := 'b0000001-0001-0001-0004-000000000001';
    v_user_picker2 UUID := 'b0000001-0001-0001-0005-000000000001';
    v_user_packer1 UUID := 'b0000001-0001-0001-0006-000000000001';
    v_user_receiver1 UUID := 'b0000001-0001-0001-0007-000000000001';
    v_user_viewer UUID := 'b0000001-0001-0001-0008-000000000001';
    -- ASP.NET Identity password hash for "Demo123!" - generated with Identity hasher
    v_password_hash TEXT := 'AQAAAAIAAYagAAAAEKVlXfVJ7jWc3P3m2xHfEVxnJqnKJ1D2HqF3+5V4N0wT2P5x6Y7Z8A9B0C1D2E3F4G==';
BEGIN
    -- ==========================================
    -- ROLES
    -- ==========================================
    -- Skip if any of the roles we want to insert already exist (check for OPERATOR since it's most common)
    IF NOT EXISTS (SELECT 1 FROM "Roles" WHERE "NormalizedName" IN ('ADMINISTRATOR', 'OPERATOR', 'VIEWER')) THEN
        INSERT INTO "Roles" (
            "Id", "Name", "NormalizedName", "Description", "TenantId",
            "IsSystemRole", "CreatedAt", "ConcurrencyStamp"
        ) VALUES
        (v_role_admin, 'Administrator', 'ADMINISTRATOR',
         'Full system access - can manage all settings, users, and data', v_tenant_id,
         true, NOW(), gen_random_uuid()::TEXT),

        (v_role_manager, 'Warehouse Manager', 'WAREHOUSE MANAGER',
         'Manage warehouse operations, users, and reports', v_tenant_id,
         true, NOW(), gen_random_uuid()::TEXT),

        (v_role_supervisor, 'Supervisor', 'SUPERVISOR',
         'Supervise daily operations and manage tasks', v_tenant_id,
         true, NOW(), gen_random_uuid()::TEXT),

        (v_role_operator, 'Operator', 'OPERATOR',
         'General warehouse operator - picking, packing, receiving', v_tenant_id,
         true, NOW(), gen_random_uuid()::TEXT),

        (v_role_picker, 'Picker', 'PICKER',
         'Pick and fulfill orders', v_tenant_id,
         true, NOW(), gen_random_uuid()::TEXT),

        (v_role_packer, 'Packer', 'PACKER',
         'Pack orders for shipment', v_tenant_id,
         true, NOW(), gen_random_uuid()::TEXT),

        (v_role_receiver, 'Receiver', 'RECEIVER',
         'Receive incoming goods', v_tenant_id,
         true, NOW(), gen_random_uuid()::TEXT),

        (v_role_viewer, 'Viewer', 'VIEWER',
         'Read-only access to reports and dashboards', v_tenant_id,
         true, NOW(), gen_random_uuid()::TEXT);

        RAISE NOTICE 'Successfully inserted 8 roles';
    ELSE
        RAISE NOTICE 'Roles already exist, skipping seed';
    END IF;

    -- ==========================================
    -- USERS
    -- ==========================================
    IF NOT EXISTS (SELECT 1 FROM "Users" WHERE "Id" = v_user_admin) THEN
        INSERT INTO "Users" (
            "Id", "TenantId",
            "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
            "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
            "PhoneNumber", "PhoneNumberConfirmed",
            "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount",
            "FirstName", "LastName",
            "IsActive", "LastLoginAt", "CreatedAt"
        ) VALUES
        -- Admin User
        (v_user_admin, v_tenant_id,
         'admin@smartwms-demo.com', 'ADMIN@SMARTWMS-DEMO.COM', 'admin@smartwms-demo.com', 'ADMIN@SMARTWMS-DEMO.COM',
         true, v_password_hash, gen_random_uuid()::TEXT, gen_random_uuid()::TEXT,
         '+46701234001', true,
         false, NULL, true, 0,
         'System', 'Administrator',
         true, NOW() - INTERVAL '1 hour', NOW()),

        -- Warehouse Manager
        (v_user_manager, v_tenant_id,
         'manager@smartwms-demo.com', 'MANAGER@SMARTWMS-DEMO.COM', 'manager@smartwms-demo.com', 'MANAGER@SMARTWMS-DEMO.COM',
         true, v_password_hash, gen_random_uuid()::TEXT, gen_random_uuid()::TEXT,
         '+46701234002', true,
         false, NULL, true, 0,
         'Erik', 'Johansson',
         true, NOW() - INTERVAL '2 hours', NOW()),

        -- Supervisor
        (v_user_supervisor, v_tenant_id,
         'supervisor@smartwms-demo.com', 'SUPERVISOR@SMARTWMS-DEMO.COM', 'supervisor@smartwms-demo.com', 'SUPERVISOR@SMARTWMS-DEMO.COM',
         true, v_password_hash, gen_random_uuid()::TEXT, gen_random_uuid()::TEXT,
         '+46701234003', true,
         false, NULL, true, 0,
         'Anna', 'Lindberg',
         true, NOW() - INTERVAL '3 hours', NOW()),

        -- Picker 1
        (v_user_picker1, v_tenant_id,
         'picker1@smartwms-demo.com', 'PICKER1@SMARTWMS-DEMO.COM', 'picker1@smartwms-demo.com', 'PICKER1@SMARTWMS-DEMO.COM',
         true, v_password_hash, gen_random_uuid()::TEXT, gen_random_uuid()::TEXT,
         '+46701234004', false,
         false, NULL, true, 0,
         'Johan', 'Andersson',
         true, NOW() - INTERVAL '1 hour', NOW()),

        -- Picker 2
        (v_user_picker2, v_tenant_id,
         'picker2@smartwms-demo.com', 'PICKER2@SMARTWMS-DEMO.COM', 'picker2@smartwms-demo.com', 'PICKER2@SMARTWMS-DEMO.COM',
         true, v_password_hash, gen_random_uuid()::TEXT, gen_random_uuid()::TEXT,
         '+46701234005', false,
         false, NULL, true, 0,
         'Maria', 'Karlsson',
         true, NOW() - INTERVAL '4 hours', NOW()),

        -- Packer 1
        (v_user_packer1, v_tenant_id,
         'packer1@smartwms-demo.com', 'PACKER1@SMARTWMS-DEMO.COM', 'packer1@smartwms-demo.com', 'PACKER1@SMARTWMS-DEMO.COM',
         true, v_password_hash, gen_random_uuid()::TEXT, gen_random_uuid()::TEXT,
         '+46701234006', false,
         false, NULL, true, 0,
         'Lars', 'Svensson',
         true, NOW() - INTERVAL '2 hours', NOW()),

        -- Receiver 1
        (v_user_receiver1, v_tenant_id,
         'receiver1@smartwms-demo.com', 'RECEIVER1@SMARTWMS-DEMO.COM', 'receiver1@smartwms-demo.com', 'RECEIVER1@SMARTWMS-DEMO.COM',
         true, v_password_hash, gen_random_uuid()::TEXT, gen_random_uuid()::TEXT,
         '+46701234007', false,
         false, NULL, true, 0,
         'Karin', 'Nilsson',
         true, NOW() - INTERVAL '5 hours', NOW()),

        -- Viewer (read-only)
        (v_user_viewer, v_tenant_id,
         'viewer@smartwms-demo.com', 'VIEWER@SMARTWMS-DEMO.COM', 'viewer@smartwms-demo.com', 'VIEWER@SMARTWMS-DEMO.COM',
         true, v_password_hash, gen_random_uuid()::TEXT, gen_random_uuid()::TEXT,
         '+46701234008', false,
         false, NULL, true, 0,
         'Guest', 'Viewer',
         true, NOW() - INTERVAL '1 day', NOW());

        RAISE NOTICE 'Successfully inserted 8 users';
    ELSE
        RAISE NOTICE 'Users already exist, skipping seed';
    END IF;

    -- ==========================================
    -- USER-ROLE ASSIGNMENTS
    -- ==========================================
    -- Look up existing roles by normalized name
    SELECT "Id" INTO v_role_admin FROM "Roles" WHERE "NormalizedName" = 'ADMIN' LIMIT 1;
    SELECT "Id" INTO v_role_manager FROM "Roles" WHERE "NormalizedName" = 'MANAGER' LIMIT 1;
    SELECT "Id" INTO v_role_operator FROM "Roles" WHERE "NormalizedName" = 'OPERATOR' LIMIT 1;
    SELECT "Id" INTO v_role_viewer FROM "Roles" WHERE "NormalizedName" = 'VIEWER' LIMIT 1;

    IF v_role_admin IS NOT NULL AND v_role_operator IS NOT NULL THEN
        -- Only insert if not already assigned
        IF NOT EXISTS (SELECT 1 FROM "AspNetUserRoles" WHERE "UserId" = v_user_admin) THEN
            INSERT INTO "AspNetUserRoles" ("UserId", "RoleId") VALUES
            (v_user_admin, v_role_admin),
            (v_user_manager, COALESCE(v_role_manager, v_role_operator)),
            (v_user_supervisor, v_role_operator),
            (v_user_picker1, v_role_operator),
            (v_user_picker2, v_role_operator),
            (v_user_packer1, v_role_operator),
            (v_user_receiver1, v_role_operator),
            (v_user_viewer, COALESCE(v_role_viewer, v_role_operator));

            RAISE NOTICE 'Successfully assigned roles to users';
        ELSE
            RAISE NOTICE 'User-role assignments already exist, skipping seed';
        END IF;
    ELSE
        RAISE NOTICE 'Required roles not found, skipping user-role assignments';
    END IF;

END $$;

-- ============================================
-- Summary of created users:
-- ============================================
-- Email                          Password    Roles
-- admin@smartwms-demo.com        Demo123!    Administrator
-- manager@smartwms-demo.com      Demo123!    Warehouse Manager
-- supervisor@smartwms-demo.com   Demo123!    Supervisor
-- picker1@smartwms-demo.com      Demo123!    Operator, Picker
-- picker2@smartwms-demo.com      Demo123!    Operator, Picker
-- packer1@smartwms-demo.com      Demo123!    Operator, Packer
-- receiver1@smartwms-demo.com    Demo123!    Operator, Receiver
-- viewer@smartwms-demo.com       Demo123!    Viewer
-- ============================================

-- Useful queries:
-- SELECT u.*, r."Name" as "RoleName" FROM "Users" u
--   LEFT JOIN "AspNetUserRoles" ur ON u."Id" = ur."UserId"
--   LEFT JOIN "Roles" r ON ur."RoleId" = r."Id"
--   WHERE u."TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
-- SELECT * FROM "Roles" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
