-- ============================================
-- SmartWMS - Seed Sessions & OperationHub Data
-- PostgreSQL
-- ============================================
-- TenantId: e9006ab8-257f-4021-b60a-cbba785bad46
-- WarehouseId: ae5e6339-6212-4435-89a9-f5940815fe20
-- ============================================
-- SessionStatus enum: 0=Active, 1=Expired, 2=Revoked, 3=Locked
-- OperatorSessionStatus enum: 0=Active, 1=OnBreak, 2=Idle, 3=Offline, 4=Ended
-- ScanType enum: 0=Barcode, 1=QRCode, 2=RFID, 3=Manual
-- ScanContext enum: 0=Login, 1=LocationVerify, 2=ProductPick, 3=ProductPutaway, 4=ProductPack, 5=ContainerScan, 6=LPNScan, 7=CycleCount, 8=Transfer, 9=Receiving, 10=Shipping, 11=Other
-- TaskAction enum: 0=Assigned, 1=Started, 2=Paused, 3=Resumed, 4=LocationArrived, 5=ProductScanned, 6=QuantityConfirmed, 7=ShortPicked, 8=Completed, 9=Cancelled, 10=Reassigned, 11=Error
-- ============================================

DO $$
DECLARE
    v_tenant_id UUID := 'e9006ab8-257f-4021-b60a-cbba785bad46';
    v_warehouse_id UUID := 'ae5e6339-6212-4435-89a9-f5940815fe20';
    v_user_ids UUID[];
    v_session_id UUID;
    v_operator_session_id UUID;
BEGIN
    -- Get users
    SELECT ARRAY_AGG("Id" ORDER BY "Email")
    INTO v_user_ids
    FROM (SELECT "Id", "Email" FROM "Users" WHERE "TenantId" = v_tenant_id AND "IsActive" = true ORDER BY "Email" LIMIT 6) u;

    IF v_user_ids IS NULL THEN
        RAISE NOTICE 'No users found, skipping sessions seed';
        RETURN;
    END IF;

    -- ============================================
    -- User Sessions
    -- ============================================
    IF NOT EXISTS (SELECT 1 FROM "UserSessions" WHERE "TenantId" = v_tenant_id LIMIT 1) THEN
        INSERT INTO "UserSessions" (
            "Id", "TenantId", "UserId",
            "SessionToken", "RefreshToken",
            "DeviceId", "DeviceName", "DeviceType", "Browser", "OperatingSystem", "AppVersion",
            "IpAddress", "Location", "Country", "City",
            "Status", "LastActivityAt", "ExpiresAt", "RevokedAt", "RevokedReason",
            "IsTrustedDevice", "FailedAttempts", "LockedUntil",
            "CreatedAt"
        ) VALUES
        -- Active sessions
        (gen_random_uuid(), v_tenant_id, v_user_ids[1],
         'session_token_admin_' || gen_random_uuid()::TEXT, 'refresh_token_admin_' || gen_random_uuid()::TEXT,
         'device-001-desktop', 'Admin Workstation', 'Desktop', 'Chrome 120', 'Windows 11', NULL,
         '192.168.1.100', 'Office - Floor 2', 'Sweden', 'Stockholm',
         0, NOW() - INTERVAL '5 minutes', NOW() + INTERVAL '7 days', NULL, NULL, -- Active
         true, 0, NULL,
         NOW() - INTERVAL '2 hours'),

        (gen_random_uuid(), v_tenant_id, v_user_ids[2],
         'session_token_manager_' || gen_random_uuid()::TEXT, 'refresh_token_manager_' || gen_random_uuid()::TEXT,
         'device-002-tablet', 'Warehouse Tablet 01', 'Tablet', 'Chrome Mobile', 'Android 13', '2.1.0',
         '192.168.1.101', 'Warehouse A', 'Sweden', 'Stockholm',
         0, NOW() - INTERVAL '10 minutes', NOW() + INTERVAL '7 days', NULL, NULL, -- Active
         true, 0, NULL,
         NOW() - INTERVAL '4 hours'),

        (gen_random_uuid(), v_tenant_id, v_user_ids[3],
         'session_token_picker_' || gen_random_uuid()::TEXT, 'refresh_token_picker_' || gen_random_uuid()::TEXT,
         'scanner-hc50-001', 'Honeywell HC50 #001', 'Scanner', NULL, 'Android 11', '2.1.0',
         '192.168.1.110', 'Pick Zone A', 'Sweden', 'Stockholm',
         0, NOW() - INTERVAL '2 minutes', NOW() + INTERVAL '8 hours', NULL, NULL, -- Active
         true, 0, NULL,
         NOW() - INTERVAL '1 hour'),

        (gen_random_uuid(), v_tenant_id, v_user_ids[4],
         'session_token_packer_' || gen_random_uuid()::TEXT, 'refresh_token_packer_' || gen_random_uuid()::TEXT,
         'device-003-desktop', 'Packing Station 1', 'Desktop', 'Edge 120', 'Windows 10', NULL,
         '192.168.1.120', 'Packing Area', 'Sweden', 'Stockholm',
         0, NOW() - INTERVAL '15 minutes', NOW() + INTERVAL '7 days', NULL, NULL, -- Active
         true, 0, NULL,
         NOW() - INTERVAL '3 hours'),

        -- Expired sessions
        (gen_random_uuid(), v_tenant_id, v_user_ids[1],
         'session_token_expired_' || gen_random_uuid()::TEXT, NULL,
         'device-001-desktop', 'Admin Workstation', 'Desktop', 'Chrome 119', 'Windows 11', NULL,
         '192.168.1.100', 'Office - Floor 2', 'Sweden', 'Stockholm',
         1, NOW() - INTERVAL '8 days', NOW() - INTERVAL '1 day', NULL, NULL, -- Expired
         true, 0, NULL,
         NOW() - INTERVAL '8 days'),

        (gen_random_uuid(), v_tenant_id, v_user_ids[3],
         'session_token_expired2_' || gen_random_uuid()::TEXT, NULL,
         'scanner-hc50-002', 'Honeywell HC50 #002', 'Scanner', NULL, 'Android 11', '2.0.5',
         '192.168.1.111', 'Pick Zone B', 'Sweden', 'Stockholm',
         1, NOW() - INTERVAL '3 days', NOW() - INTERVAL '2 days', NULL, NULL, -- Expired
         false, 0, NULL,
         NOW() - INTERVAL '5 days'),

        -- Revoked session (security concern)
        (gen_random_uuid(), v_tenant_id, v_user_ids[5],
         'session_token_revoked_' || gen_random_uuid()::TEXT, NULL,
         'unknown-device', 'Unknown Device', 'Mobile', 'Chrome Mobile', 'Android 14', NULL,
         '10.0.0.55', 'Unknown Location', NULL, NULL,
         2, NOW() - INTERVAL '2 days', NOW() + INTERVAL '5 days', NOW() - INTERVAL '2 days', 'Suspicious login from unknown location', -- Revoked
         false, 3, NULL,
         NOW() - INTERVAL '2 days'),

        -- Mobile session
        (gen_random_uuid(), v_tenant_id, v_user_ids[2],
         'session_token_mobile_' || gen_random_uuid()::TEXT, 'refresh_token_mobile_' || gen_random_uuid()::TEXT,
         'device-mobile-001', 'Manager iPhone', 'Mobile', 'Safari', 'iOS 17', '2.1.0',
         '192.168.1.150', 'Mobile', 'Sweden', 'Stockholm',
         0, NOW() - INTERVAL '1 hour', NOW() + INTERVAL '30 days', NULL, NULL, -- Active
         true, 0, NULL,
         NOW() - INTERVAL '5 days');

        RAISE NOTICE 'Successfully inserted 8 user sessions';
    ELSE
        RAISE NOTICE 'User sessions already exist, skipping seed';
    END IF;

    -- ============================================
    -- Login Attempts
    -- ============================================
    IF NOT EXISTS (SELECT 1 FROM "LoginAttempts" WHERE "TenantId" = v_tenant_id LIMIT 1) THEN
        INSERT INTO "LoginAttempts" (
            "Id", "TenantId",
            "UserId", "UserName", "Email",
            "Success", "FailureReason",
            "IpAddress", "UserAgent", "Location",
            "CreatedAt"
        ) VALUES
        -- Successful logins
        (gen_random_uuid(), v_tenant_id,
         v_user_ids[1], 'admin', 'admin@example.com',
         true, NULL,
         '192.168.1.100', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/120.0.0.0', 'Stockholm, Sweden',
         NOW() - INTERVAL '2 hours'),

        (gen_random_uuid(), v_tenant_id,
         v_user_ids[2], 'warehouse_manager', 'manager@example.com',
         true, NULL,
         '192.168.1.101', 'Mozilla/5.0 (Linux; Android 13) Chrome/120.0.0.0', 'Stockholm, Sweden',
         NOW() - INTERVAL '4 hours'),

        (gen_random_uuid(), v_tenant_id,
         v_user_ids[3], 'picker01', 'picker01@example.com',
         true, NULL,
         '192.168.1.110', 'SmartWMS-Scanner/2.1.0 (Android 11)', 'Stockholm, Sweden',
         NOW() - INTERVAL '1 hour'),

        -- Failed login attempts
        (gen_random_uuid(), v_tenant_id,
         NULL, 'admin', 'admin@example.com',
         false, 'Invalid password',
         '192.168.1.100', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/120.0.0.0', 'Stockholm, Sweden',
         NOW() - INTERVAL '2 hours' - INTERVAL '5 minutes'),

        (gen_random_uuid(), v_tenant_id,
         NULL, NULL, 'unknown@example.com',
         false, 'User not found',
         '10.0.0.55', 'Mozilla/5.0 (Linux; Android 12) Chrome/120.0.0.0', 'Unknown',
         NOW() - INTERVAL '3 hours'),

        (gen_random_uuid(), v_tenant_id,
         NULL, NULL, 'test@example.com',
         false, 'User not found',
         '10.0.0.55', 'Mozilla/5.0 (Linux; Android 12) Chrome/120.0.0.0', 'Unknown',
         NOW() - INTERVAL '3 hours' - INTERVAL '1 minute'),

        (gen_random_uuid(), v_tenant_id,
         NULL, NULL, 'admin@wrongdomain.com',
         false, 'User not found',
         '10.0.0.55', 'Mozilla/5.0 (Linux; Android 12) Chrome/120.0.0.0', 'Unknown',
         NOW() - INTERVAL '3 hours' - INTERVAL '2 minutes'),

        -- Successful after failed
        (gen_random_uuid(), v_tenant_id,
         v_user_ids[4], 'packer01', 'packer01@example.com',
         true, NULL,
         '192.168.1.120', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) Edge/120.0.0.0', 'Stockholm, Sweden',
         NOW() - INTERVAL '3 hours'),

        (gen_random_uuid(), v_tenant_id,
         NULL, 'packer01', 'packer01@example.com',
         false, 'Invalid password - Caps Lock may be on',
         '192.168.1.120', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) Edge/120.0.0.0', 'Stockholm, Sweden',
         NOW() - INTERVAL '3 hours' - INTERVAL '1 minute');

        RAISE NOTICE 'Successfully inserted 9 login attempts';
    ELSE
        RAISE NOTICE 'Login attempts already exist, skipping seed';
    END IF;

    -- ============================================
    -- Trusted Devices
    -- ============================================
    IF NOT EXISTS (SELECT 1 FROM "TrustedDevices" WHERE "TenantId" = v_tenant_id LIMIT 1) THEN
        INSERT INTO "TrustedDevices" (
            "Id", "TenantId", "UserId",
            "DeviceId", "DeviceName", "DeviceType",
            "LastUsedAt", "LastIpAddress", "IsActive",
            "CreatedAt"
        ) VALUES
        (gen_random_uuid(), v_tenant_id, v_user_ids[1],
         'device-001-desktop', 'Admin Workstation', 'Desktop',
         NOW() - INTERVAL '5 minutes', '192.168.1.100', true,
         NOW() - INTERVAL '30 days'),

        (gen_random_uuid(), v_tenant_id, v_user_ids[2],
         'device-002-tablet', 'Warehouse Tablet 01', 'Tablet',
         NOW() - INTERVAL '10 minutes', '192.168.1.101', true,
         NOW() - INTERVAL '20 days'),

        (gen_random_uuid(), v_tenant_id, v_user_ids[2],
         'device-mobile-001', 'Manager iPhone', 'Mobile',
         NOW() - INTERVAL '1 hour', '192.168.1.150', true,
         NOW() - INTERVAL '5 days'),

        (gen_random_uuid(), v_tenant_id, v_user_ids[3],
         'scanner-hc50-001', 'Honeywell HC50 #001', 'Scanner',
         NOW() - INTERVAL '2 minutes', '192.168.1.110', true,
         NOW() - INTERVAL '60 days'),

        (gen_random_uuid(), v_tenant_id, v_user_ids[3],
         'scanner-hc50-002', 'Honeywell HC50 #002', 'Scanner',
         NOW() - INTERVAL '3 days', '192.168.1.111', true,
         NOW() - INTERVAL '45 days'),

        (gen_random_uuid(), v_tenant_id, v_user_ids[4],
         'device-003-desktop', 'Packing Station 1', 'Desktop',
         NOW() - INTERVAL '15 minutes', '192.168.1.120', true,
         NOW() - INTERVAL '90 days'),

        -- Inactive/old device
        (gen_random_uuid(), v_tenant_id, v_user_ids[1],
         'device-old-laptop', 'Old Laptop', 'Desktop',
         NOW() - INTERVAL '90 days', '192.168.1.200', false,
         NOW() - INTERVAL '180 days');

        RAISE NOTICE 'Successfully inserted 7 trusted devices';
    ELSE
        RAISE NOTICE 'Trusted devices already exist, skipping seed';
    END IF;

    -- ============================================
    -- Operator Sessions
    -- ============================================
    IF NOT EXISTS (SELECT 1 FROM "OperatorSessions" WHERE "TenantId" = v_tenant_id LIMIT 1) THEN
        v_operator_session_id := gen_random_uuid();

        INSERT INTO "OperatorSessions" (
            "Id", "TenantId", "UserId", "WarehouseId",
            "DeviceId", "DeviceType", "DeviceName",
            "Status", "StartedAt", "EndedAt", "LastActivityAt",
            "CurrentTaskType", "CurrentTaskId", "CurrentZone", "CurrentLocation",
            "ShiftCode", "ShiftStart", "ShiftEnd",
            "CreatedAt"
        ) VALUES
        -- Active operator sessions
        (v_operator_session_id, v_tenant_id, v_user_ids[3], v_warehouse_id,
         'scanner-hc50-001', 'Scanner', 'Honeywell HC50 #001',
         0, NOW() - INTERVAL '3 hours', NULL, NOW() - INTERVAL '2 minutes', -- Active
         'Pick', NULL, 'PCK-A', 'PICK-A-05',
         'SHIFT-A', NOW() - INTERVAL '3 hours', NOW() + INTERVAL '5 hours',
         NOW() - INTERVAL '3 hours'),

        (gen_random_uuid(), v_tenant_id, v_user_ids[4], v_warehouse_id,
         'device-003-desktop', 'Desktop', 'Packing Station 1',
         0, NOW() - INTERVAL '4 hours', NULL, NOW() - INTERVAL '15 minutes', -- Active
         'Pack', NULL, 'PACK', 'PACK-ST-01',
         'SHIFT-A', NOW() - INTERVAL '4 hours', NOW() + INTERVAL '4 hours',
         NOW() - INTERVAL '4 hours'),

        (gen_random_uuid(), v_tenant_id, v_user_ids[5], v_warehouse_id,
         'scanner-hc50-003', 'Scanner', 'Honeywell HC50 #003',
         1, NOW() - INTERVAL '2 hours', NULL, NOW() - INTERVAL '20 minutes', -- OnBreak
         'Putaway', NULL, 'BLK-A', NULL,
         'SHIFT-A', NOW() - INTERVAL '4 hours', NOW() + INTERVAL '4 hours',
         NOW() - INTERVAL '2 hours'),

        -- Idle session
        (gen_random_uuid(), v_tenant_id, v_user_ids[6], v_warehouse_id,
         'scanner-hc50-004', 'Scanner', 'Honeywell HC50 #004',
         2, NOW() - INTERVAL '5 hours', NULL, NOW() - INTERVAL '45 minutes', -- Idle
         NULL, NULL, NULL, NULL,
         'SHIFT-A', NOW() - INTERVAL '5 hours', NOW() + INTERVAL '3 hours',
         NOW() - INTERVAL '5 hours'),

        -- Ended sessions (historical)
        (gen_random_uuid(), v_tenant_id, v_user_ids[3], v_warehouse_id,
         'scanner-hc50-001', 'Scanner', 'Honeywell HC50 #001',
         4, NOW() - INTERVAL '1 day' - INTERVAL '8 hours', NOW() - INTERVAL '1 day', NOW() - INTERVAL '1 day', -- Ended
         NULL, NULL, NULL, NULL,
         'SHIFT-A', NOW() - INTERVAL '1 day' - INTERVAL '8 hours', NOW() - INTERVAL '1 day',
         NOW() - INTERVAL '1 day' - INTERVAL '8 hours'),

        (gen_random_uuid(), v_tenant_id, v_user_ids[4], v_warehouse_id,
         'device-003-desktop', 'Desktop', 'Packing Station 1',
         4, NOW() - INTERVAL '1 day' - INTERVAL '9 hours', NOW() - INTERVAL '1 day' - INTERVAL '1 hour', NOW() - INTERVAL '1 day' - INTERVAL '1 hour', -- Ended
         NULL, NULL, NULL, NULL,
         'SHIFT-A', NOW() - INTERVAL '1 day' - INTERVAL '9 hours', NOW() - INTERVAL '1 day' - INTERVAL '1 hour',
         NOW() - INTERVAL '1 day' - INTERVAL '9 hours');

        RAISE NOTICE 'Successfully inserted 6 operator sessions';
    ELSE
        RAISE NOTICE 'Operator sessions already exist, skipping seed';
    END IF;

    -- ============================================
    -- Operator Productivity (Daily Stats)
    -- ============================================
    IF NOT EXISTS (SELECT 1 FROM "OperatorProductivity" WHERE "TenantId" = v_tenant_id LIMIT 1) THEN
        INSERT INTO "OperatorProductivity" (
            "Id", "TenantId", "UserId", "WarehouseId", "Date",
            "PickTasksCompleted", "PackTasksCompleted", "PutawayTasksCompleted", "CycleCountsCompleted",
            "TotalUnitsPicked", "TotalUnitsPacked", "TotalUnitsPutaway", "TotalLocationsVisited",
            "TotalWorkMinutes", "TotalIdleMinutes", "TotalBreakMinutes",
            "TotalScans", "CorrectScans", "ErrorScans", "AccuracyRate",
            "PicksPerHour", "UnitsPerHour", "TasksPerHour",
            "CreatedAt"
        ) VALUES
        -- Today's stats (partial day)
        (gen_random_uuid(), v_tenant_id, v_user_ids[3], v_warehouse_id, CURRENT_DATE,
         12, 0, 3, 0,
         156, 0, 45, 28,
         180, 15, 30,
         245, 242, 3, 98.78,
         15.2, 52.0, 5.0,
         NOW()),

        (gen_random_uuid(), v_tenant_id, v_user_ids[4], v_warehouse_id, CURRENT_DATE,
         0, 18, 0, 0,
         0, 234, 0, 0,
         240, 10, 30,
         180, 180, 0, 100.0,
         0, 58.5, 4.5,
         NOW()),

        (gen_random_uuid(), v_tenant_id, v_user_ids[5], v_warehouse_id, CURRENT_DATE,
         0, 0, 8, 0,
         0, 0, 120, 15,
         120, 20, 15,
         95, 93, 2, 97.89,
         0, 60.0, 4.0,
         NOW()),

        -- Yesterday's stats
        (gen_random_uuid(), v_tenant_id, v_user_ids[3], v_warehouse_id, CURRENT_DATE - 1,
         25, 0, 5, 2,
         312, 0, 78, 52,
         420, 30, 60,
         520, 515, 5, 99.04,
         18.1, 58.0, 4.6,
         NOW()),

        (gen_random_uuid(), v_tenant_id, v_user_ids[4], v_warehouse_id, CURRENT_DATE - 1,
         0, 35, 0, 0,
         0, 456, 0, 0,
         450, 20, 50,
         350, 348, 2, 99.43,
         0, 60.8, 4.7,
         NOW()),

        -- 2 days ago
        (gen_random_uuid(), v_tenant_id, v_user_ids[3], v_warehouse_id, CURRENT_DATE - 2,
         22, 0, 7, 0,
         278, 0, 95, 48,
         400, 35, 55,
         480, 475, 5, 98.96,
         16.5, 52.3, 4.4,
         NOW()),

        -- 3 days ago (lower productivity - was training new staff)
        (gen_random_uuid(), v_tenant_id, v_user_ids[3], v_warehouse_id, CURRENT_DATE - 3,
         10, 0, 2, 5,
         125, 0, 30, 25,
         360, 60, 60,
         200, 195, 5, 97.5,
         8.3, 34.7, 2.8,
         NOW());

        RAISE NOTICE 'Successfully inserted 7 operator productivity records';
    ELSE
        RAISE NOTICE 'Operator productivity already exists, skipping seed';
    END IF;

    -- ============================================
    -- Scan Logs
    -- ============================================
    IF NOT EXISTS (SELECT 1 FROM "ScanLogs" WHERE "TenantId" = v_tenant_id LIMIT 1) THEN
        INSERT INTO "ScanLogs" (
            "Id", "TenantId", "UserId", "SessionId", "WarehouseId",
            "Barcode", "ScanType", "Context",
            "EntityType", "EntityId", "ResolvedSku", "ResolvedLocation",
            "TaskType", "TaskId",
            "Success", "ErrorCode", "ErrorMessage",
            "DeviceId", "ScannedAt",
            "CreatedAt"
        ) VALUES
        -- Recent picking scans
        (gen_random_uuid(), v_tenant_id, v_user_ids[3], NULL, v_warehouse_id,
         'PICK-A-05', 0, 1, -- Barcode, LocationVerify
         'Location', NULL, NULL, 'PICK-A-05',
         'Pick', NULL,
         true, NULL, NULL,
         'scanner-hc50-001', NOW() - INTERVAL '5 minutes',
         NOW() - INTERVAL '5 minutes'),

        (gen_random_uuid(), v_tenant_id, v_user_ids[3], NULL, v_warehouse_id,
         '4901234567890', 0, 2, -- Barcode, ProductPick
         'Product', NULL, 'PROD-001', 'PICK-A-05',
         'Pick', NULL,
         true, NULL, NULL,
         'scanner-hc50-001', NOW() - INTERVAL '4 minutes',
         NOW() - INTERVAL '4 minutes'),

        (gen_random_uuid(), v_tenant_id, v_user_ids[3], NULL, v_warehouse_id,
         'PICK-A-06', 0, 1, -- Barcode, LocationVerify
         'Location', NULL, NULL, 'PICK-A-06',
         'Pick', NULL,
         true, NULL, NULL,
         'scanner-hc50-001', NOW() - INTERVAL '3 minutes',
         NOW() - INTERVAL '3 minutes'),

        (gen_random_uuid(), v_tenant_id, v_user_ids[3], NULL, v_warehouse_id,
         '4901234567891', 0, 2, -- Barcode, ProductPick
         'Product', NULL, 'PROD-002', 'PICK-A-06',
         'Pick', NULL,
         true, NULL, NULL,
         'scanner-hc50-001', NOW() - INTERVAL '2 minutes',
         NOW() - INTERVAL '2 minutes'),

        -- Error scan (wrong location)
        (gen_random_uuid(), v_tenant_id, v_user_ids[3], NULL, v_warehouse_id,
         'PICK-B-01', 0, 1, -- Barcode, LocationVerify
         'Location', NULL, NULL, 'PICK-B-01',
         'Pick', NULL,
         false, 'WRONG_LOCATION', 'Expected location PICK-A-07, scanned PICK-B-01',
         'scanner-hc50-001', NOW() - INTERVAL '10 minutes',
         NOW() - INTERVAL '10 minutes'),

        -- Putaway scans
        (gen_random_uuid(), v_tenant_id, v_user_ids[5], NULL, v_warehouse_id,
         'BLK-A-02-03', 0, 3, -- Barcode, ProductPutaway
         'Location', NULL, NULL, 'BLK-A-02-03',
         'Putaway', NULL,
         true, NULL, NULL,
         'scanner-hc50-003', NOW() - INTERVAL '25 minutes',
         NOW() - INTERVAL '25 minutes'),

        (gen_random_uuid(), v_tenant_id, v_user_ids[5], NULL, v_warehouse_id,
         '4901234567895', 0, 3, -- Barcode, ProductPutaway
         'Product', NULL, 'PROD-005', 'BLK-A-02-03',
         'Putaway', NULL,
         true, NULL, NULL,
         'scanner-hc50-003', NOW() - INTERVAL '24 minutes',
         NOW() - INTERVAL '24 minutes'),

        -- Receiving scans
        (gen_random_uuid(), v_tenant_id, v_user_ids[3], NULL, v_warehouse_id,
         'RCV-DOCK-01', 0, 9, -- Barcode, Receiving
         'Location', NULL, NULL, 'RCV-DOCK-01',
         'Receiving', NULL,
         true, NULL, NULL,
         'scanner-hc50-001', NOW() - INTERVAL '1 hour',
         NOW() - INTERVAL '1 hour'),

        (gen_random_uuid(), v_tenant_id, v_user_ids[3], NULL, v_warehouse_id,
         '4901234567893', 0, 9, -- Barcode, Receiving
         'Product', NULL, 'PROD-003', 'RCV-DOCK-01',
         'Receiving', NULL,
         true, NULL, NULL,
         'scanner-hc50-001', NOW() - INTERVAL '59 minutes',
         NOW() - INTERVAL '59 minutes'),

        -- Packing scan (LPN)
        (gen_random_uuid(), v_tenant_id, v_user_ids[4], NULL, v_warehouse_id,
         'LPN-2024-00567', 0, 6, -- Barcode, LPNScan
         'Container', NULL, NULL, 'PACK-ST-01',
         'Pack', NULL,
         true, NULL, NULL,
         'device-003-desktop', NOW() - INTERVAL '20 minutes',
         NOW() - INTERVAL '20 minutes'),

        -- QR code scan
        (gen_random_uuid(), v_tenant_id, v_user_ids[4], NULL, v_warehouse_id,
         'QR-SHIP-LABEL-001', 1, 10, -- QRCode, Shipping
         'Shipment', NULL, NULL, 'SHP-DOCK-01',
         'Pack', NULL,
         true, NULL, NULL,
         'device-003-desktop', NOW() - INTERVAL '18 minutes',
         NOW() - INTERVAL '18 minutes'),

        -- Login scan (badge)
        (gen_random_uuid(), v_tenant_id, v_user_ids[3], NULL, v_warehouse_id,
         'BADGE-EMP-003', 0, 0, -- Barcode, Login
         'User', v_user_ids[3], NULL, NULL,
         NULL, NULL,
         true, NULL, NULL,
         'scanner-hc50-001', NOW() - INTERVAL '3 hours',
         NOW() - INTERVAL '3 hours');

        RAISE NOTICE 'Successfully inserted 12 scan logs';
    ELSE
        RAISE NOTICE 'Scan logs already exist, skipping seed';
    END IF;

    -- ============================================
    -- Task Action Logs
    -- ============================================
    IF NOT EXISTS (SELECT 1 FROM "TaskActionLogs" WHERE "TenantId" = v_tenant_id LIMIT 1) THEN
        INSERT INTO "TaskActionLogs" (
            "Id", "TenantId", "UserId", "WarehouseId",
            "TaskType", "TaskId", "TaskNumber",
            "Action", "ActionAt",
            "FromStatus", "ToStatus", "LocationCode", "ProductSku", "Quantity",
            "DurationSeconds",
            "Notes", "ReasonCode",
            "CreatedAt"
        ) VALUES
        -- Picking task flow
        (gen_random_uuid(), v_tenant_id, v_user_ids[3], v_warehouse_id,
         'Pick', gen_random_uuid(), 'PICK-2024-0101',
         0, NOW() - INTERVAL '30 minutes', -- Assigned
         NULL, 'Assigned', NULL, NULL, NULL,
         NULL,
         'Auto-assigned by system', NULL,
         NOW() - INTERVAL '30 minutes'),

        (gen_random_uuid(), v_tenant_id, v_user_ids[3], v_warehouse_id,
         'Pick', gen_random_uuid(), 'PICK-2024-0101',
         1, NOW() - INTERVAL '28 minutes', -- Started
         'Assigned', 'InProgress', NULL, NULL, NULL,
         120,
         NULL, NULL,
         NOW() - INTERVAL '28 minutes'),

        (gen_random_uuid(), v_tenant_id, v_user_ids[3], v_warehouse_id,
         'Pick', gen_random_uuid(), 'PICK-2024-0101',
         4, NOW() - INTERVAL '25 minutes', -- LocationArrived
         NULL, NULL, 'PICK-A-05', NULL, NULL,
         180,
         NULL, NULL,
         NOW() - INTERVAL '25 minutes'),

        (gen_random_uuid(), v_tenant_id, v_user_ids[3], v_warehouse_id,
         'Pick', gen_random_uuid(), 'PICK-2024-0101',
         5, NOW() - INTERVAL '24 minutes', -- ProductScanned
         NULL, NULL, 'PICK-A-05', 'PROD-001', NULL,
         60,
         NULL, NULL,
         NOW() - INTERVAL '24 minutes'),

        (gen_random_uuid(), v_tenant_id, v_user_ids[3], v_warehouse_id,
         'Pick', gen_random_uuid(), 'PICK-2024-0101',
         6, NOW() - INTERVAL '23 minutes', -- QuantityConfirmed
         NULL, NULL, 'PICK-A-05', 'PROD-001', 10,
         60,
         NULL, NULL,
         NOW() - INTERVAL '23 minutes'),

        (gen_random_uuid(), v_tenant_id, v_user_ids[3], v_warehouse_id,
         'Pick', gen_random_uuid(), 'PICK-2024-0101',
         8, NOW() - INTERVAL '15 minutes', -- Completed
         'InProgress', 'Complete', NULL, NULL, NULL,
         480,
         'All items picked', NULL,
         NOW() - INTERVAL '15 minutes'),

        -- Short pick scenario
        (gen_random_uuid(), v_tenant_id, v_user_ids[3], v_warehouse_id,
         'Pick', gen_random_uuid(), 'PICK-2024-0100',
         7, NOW() - INTERVAL '1 hour', -- ShortPicked
         NULL, NULL, 'PICK-A-03', 'PROD-003', 5,
         NULL,
         'Only 5 units available, expected 10', 'SHORT_STOCK',
         NOW() - INTERVAL '1 hour'),

        -- Paused and resumed task
        (gen_random_uuid(), v_tenant_id, v_user_ids[5], v_warehouse_id,
         'Putaway', gen_random_uuid(), 'PUT-2024-0050',
         2, NOW() - INTERVAL '25 minutes', -- Paused
         'InProgress', 'Paused', 'BLK-A-02', NULL, NULL,
         NULL,
         'Taking break', NULL,
         NOW() - INTERVAL '25 minutes'),

        (gen_random_uuid(), v_tenant_id, v_user_ids[5], v_warehouse_id,
         'Putaway', gen_random_uuid(), 'PUT-2024-0050',
         3, NOW() - INTERVAL '5 minutes', -- Resumed
         'Paused', 'InProgress', 'BLK-A-02', NULL, NULL,
         1200, -- 20 minute break
         'Back from break', NULL,
         NOW() - INTERVAL '5 minutes'),

        -- Cancelled task
        (gen_random_uuid(), v_tenant_id, v_user_ids[4], v_warehouse_id,
         'Pack', gen_random_uuid(), 'PACK-2024-0099',
         9, NOW() - INTERVAL '2 hours', -- Cancelled
         'Assigned', 'Cancelled', NULL, NULL, NULL,
         NULL,
         'Order cancelled by customer', 'ORDER_CANCELLED',
         NOW() - INTERVAL '2 hours'),

        -- Reassigned task
        (gen_random_uuid(), v_tenant_id, v_user_ids[2], v_warehouse_id,
         'Pick', gen_random_uuid(), 'PICK-2024-0098',
         10, NOW() - INTERVAL '3 hours', -- Reassigned
         'Assigned', 'Assigned', NULL, NULL, NULL,
         NULL,
         'Reassigned from picker03 to picker01 due to shift end', NULL,
         NOW() - INTERVAL '3 hours'),

        -- Error during task
        (gen_random_uuid(), v_tenant_id, v_user_ids[3], v_warehouse_id,
         'Pick', gen_random_uuid(), 'PICK-2024-0097',
         11, NOW() - INTERVAL '4 hours', -- Error
         NULL, NULL, 'PICK-B-01', 'PROD-007', NULL,
         NULL,
         'Wrong location scanned', 'WRONG_LOCATION',
         NOW() - INTERVAL '4 hours');

        RAISE NOTICE 'Successfully inserted 12 task action logs';
    ELSE
        RAISE NOTICE 'Task action logs already exist, skipping seed';
    END IF;

END $$;

-- Useful queries:
-- SELECT * FROM "UserSessions" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46' ORDER BY "CreatedAt" DESC;
-- SELECT * FROM "LoginAttempts" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46' ORDER BY "CreatedAt" DESC;
-- SELECT * FROM "TrustedDevices" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
-- SELECT * FROM "OperatorSessions" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46' ORDER BY "StartedAt" DESC;
-- SELECT * FROM "OperatorProductivity" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46' ORDER BY "Date" DESC;
-- SELECT * FROM "ScanLogs" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46' ORDER BY "ScannedAt" DESC LIMIT 50;
-- SELECT * FROM "TaskActionLogs" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46' ORDER BY "ActionAt" DESC LIMIT 50;
