-- ============================================
-- SmartWMS - Seed Audit/Logs Data
-- PostgreSQL
-- ============================================
-- TenantId: e9006ab8-257f-4021-b60a-cbba785bad46
-- ============================================
-- AuditSeverity enum: 0=Debug, 1=Info, 2=Warning, 3=Error, 4=Critical
-- SystemEventCategory enum: 0=Application, 1=Security, 2=Performance, 3=Integration, 4=Scheduler, 5=Database, 6=FileSystem, 7=Network, 8=Other
-- SystemEventSeverity enum: 0=Trace, 1=Debug, 2=Information, 3=Warning, 4=Error, 5=Critical, 6=Fatal
-- ============================================

DO $$
DECLARE
    v_tenant_id UUID := 'e9006ab8-257f-4021-b60a-cbba785bad46';
    v_user_ids UUID[];
    v_user_emails TEXT[];
    v_product_ids UUID[];
    v_order_ids UUID[];
    v_correlation_id UUID;
BEGIN
    -- Get users
    SELECT ARRAY_AGG("Id" ORDER BY "Email"), ARRAY_AGG("Email" ORDER BY "Email")
    INTO v_user_ids, v_user_emails
    FROM (SELECT "Id", "Email" FROM "Users" WHERE "TenantId" = v_tenant_id AND "IsActive" = true ORDER BY "Email" LIMIT 5) u;

    -- Get products
    SELECT ARRAY_AGG("Id" ORDER BY "Sku")
    INTO v_product_ids
    FROM (SELECT "Id", "Sku" FROM "Products" WHERE "TenantId" = v_tenant_id AND "IsActive" = true ORDER BY "Sku" LIMIT 5) p;

    -- Get sales orders
    SELECT ARRAY_AGG("Id" ORDER BY "OrderNumber")
    INTO v_order_ids
    FROM (SELECT "Id", "OrderNumber" FROM "SalesOrders" WHERE "TenantId" = v_tenant_id ORDER BY "OrderNumber" LIMIT 5) so;

    -- ============================================
    -- Audit Logs
    -- ============================================
    IF NOT EXISTS (SELECT 1 FROM "AuditLogs" WHERE "TenantId" = v_tenant_id LIMIT 1) THEN
        v_correlation_id := gen_random_uuid();

        INSERT INTO "AuditLogs" (
            "Id", "TenantId",
            "EventType", "EntityType", "EntityId", "EntityNumber",
            "Action", "Severity",
            "UserId", "UserName", "UserEmail",
            "EventTime", "IpAddress", "UserAgent",
            "OldValues", "NewValues", "ChangedFields",
            "Module", "SubModule", "CorrelationId", "SessionId",
            "Notes", "ErrorMessage", "IsSuccess",
            "CreatedAt"
        ) VALUES
        -- User logins
        (gen_random_uuid(), v_tenant_id,
         'Login', 'User', v_user_ids[1], NULL,
         'User logged in successfully', 1, -- Info
         v_user_ids[1], 'Admin User', v_user_emails[1],
         NOW() - INTERVAL '2 hours', '192.168.1.100', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/120.0.0.0',
         NULL, NULL, NULL,
         'Users', 'Authentication', gen_random_uuid()::TEXT, gen_random_uuid()::TEXT,
         NULL, NULL, true,
         NOW() - INTERVAL '2 hours'),

        (gen_random_uuid(), v_tenant_id,
         'Login', 'User', v_user_ids[2], NULL,
         'User logged in successfully', 1, -- Info
         v_user_ids[2], 'Warehouse Manager', v_user_emails[2],
         NOW() - INTERVAL '1 hour', '192.168.1.101', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/120.0.0.0',
         NULL, NULL, NULL,
         'Users', 'Authentication', gen_random_uuid()::TEXT, gen_random_uuid()::TEXT,
         NULL, NULL, true,
         NOW() - INTERVAL '1 hour'),

        (gen_random_uuid(), v_tenant_id,
         'LoginFailed', 'User', NULL, NULL,
         'Failed login attempt - invalid password', 2, -- Warning
         NULL, NULL, 'unknown@example.com',
         NOW() - INTERVAL '3 hours', '10.0.0.55', 'Mozilla/5.0 (Linux; Android 12) Chrome/120.0.0.0',
         NULL, NULL, NULL,
         'Users', 'Authentication', gen_random_uuid()::TEXT, NULL,
         'Multiple failed attempts from this IP', NULL, false,
         NOW() - INTERVAL '3 hours'),

        -- Product changes
        (gen_random_uuid(), v_tenant_id,
         'Update', 'Product', v_product_ids[1], 'PROD-001',
         'Updated product details', 1, -- Info
         v_user_ids[1], 'Admin User', v_user_emails[1],
         NOW() - INTERVAL '5 hours', '192.168.1.100', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/120.0.0.0',
         '{"Description": "Old description", "MinStockLevel": 50}',
         '{"Description": "Updated product description", "MinStockLevel": 75}',
         'Description,MinStockLevel',
         'Inventory', 'Products', v_correlation_id::TEXT, gen_random_uuid()::TEXT,
         NULL, NULL, true,
         NOW() - INTERVAL '5 hours'),

        (gen_random_uuid(), v_tenant_id,
         'Create', 'Product', v_product_ids[3], 'PROD-003',
         'Created new product', 1, -- Info
         v_user_ids[1], 'Admin User', v_user_emails[1],
         NOW() - INTERVAL '2 days', '192.168.1.100', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/120.0.0.0',
         NULL,
         '{"Sku": "PROD-003", "Name": "New Product", "UnitOfMeasure": "EA"}',
         NULL,
         'Inventory', 'Products', gen_random_uuid()::TEXT, gen_random_uuid()::TEXT,
         NULL, NULL, true,
         NOW() - INTERVAL '2 days'),

        -- Order operations
        (gen_random_uuid(), v_tenant_id,
         'StatusChanged', 'SalesOrder', v_order_ids[1], 'SO-2024-0001',
         'Order status changed from Pending to Confirmed', 1, -- Info
         v_user_ids[2], 'Warehouse Manager', v_user_emails[2],
         NOW() - INTERVAL '1 day', '192.168.1.101', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/120.0.0.0',
         '{"Status": 0}', '{"Status": 1}', 'Status',
         'Orders', 'SalesOrders', gen_random_uuid()::TEXT, gen_random_uuid()::TEXT,
         'Order confirmed for processing', NULL, true,
         NOW() - INTERVAL '1 day'),

        (gen_random_uuid(), v_tenant_id,
         'Create', 'SalesOrder', v_order_ids[2], 'SO-2024-0002',
         'Created new sales order', 1, -- Info
         v_user_ids[1], 'Admin User', v_user_emails[1],
         NOW() - INTERVAL '2 days', '192.168.1.100', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/120.0.0.0',
         NULL,
         '{"OrderNumber": "SO-2024-0002", "Status": 0, "TotalLines": 3}',
         NULL,
         'Orders', 'SalesOrders', gen_random_uuid()::TEXT, gen_random_uuid()::TEXT,
         NULL, NULL, true,
         NOW() - INTERVAL '2 days'),

        -- Stock adjustments
        (gen_random_uuid(), v_tenant_id,
         'Create', 'StockAdjustment', NULL, 'ADJ-2024-0001',
         'Created stock adjustment for damaged goods', 2, -- Warning
         v_user_ids[2], 'Warehouse Manager', v_user_emails[2],
         NOW() - INTERVAL '3 days', '192.168.1.101', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/120.0.0.0',
         NULL,
         '{"AdjustmentNumber": "ADJ-2024-0001", "ReasonCode": "DAMAGED", "TotalQuantity": -15}',
         NULL,
         'Inventory', 'Adjustments', gen_random_uuid()::TEXT, gen_random_uuid()::TEXT,
         'Damaged items found during cycle count', NULL, true,
         NOW() - INTERVAL '3 days'),

        -- User management
        (gen_random_uuid(), v_tenant_id,
         'Create', 'User', v_user_ids[3], NULL,
         'Created new user account', 1, -- Info
         v_user_ids[1], 'Admin User', v_user_emails[1],
         NOW() - INTERVAL '7 days', '192.168.1.100', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/120.0.0.0',
         NULL,
         '{"Email": "' || v_user_emails[3] || '", "Roles": ["Operator"]}',
         NULL,
         'Users', 'UserManagement', gen_random_uuid()::TEXT, gen_random_uuid()::TEXT,
         NULL, NULL, true,
         NOW() - INTERVAL '7 days'),

        (gen_random_uuid(), v_tenant_id,
         'Update', 'User', v_user_ids[3], NULL,
         'Updated user roles', 1, -- Info
         v_user_ids[1], 'Admin User', v_user_emails[1],
         NOW() - INTERVAL '5 days', '192.168.1.100', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/120.0.0.0',
         '{"Roles": ["Operator"]}',
         '{"Roles": ["Operator", "Picker"]}',
         'Roles',
         'Users', 'UserManagement', gen_random_uuid()::TEXT, gen_random_uuid()::TEXT,
         'Added picker role', NULL, true,
         NOW() - INTERVAL '5 days'),

        -- Failed operations
        (gen_random_uuid(), v_tenant_id,
         'Delete', 'Product', v_product_ids[2], 'PROD-002',
         'Attempted to delete product with active stock', 3, -- Error
         v_user_ids[1], 'Admin User', v_user_emails[1],
         NOW() - INTERVAL '4 hours', '192.168.1.100', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/120.0.0.0',
         NULL, NULL, NULL,
         'Inventory', 'Products', gen_random_uuid()::TEXT, gen_random_uuid()::TEXT,
         NULL, 'Cannot delete product with active stock levels', false,
         NOW() - INTERVAL '4 hours'),

        -- Critical security event
        (gen_random_uuid(), v_tenant_id,
         'PermissionDenied', 'SystemSettings', NULL, NULL,
         'Unauthorized attempt to modify system settings', 4, -- Critical
         v_user_ids[3], 'Operator', v_user_emails[3],
         NOW() - INTERVAL '6 hours', '192.168.1.105', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/120.0.0.0',
         NULL, NULL, NULL,
         'Config', 'SystemSettings', gen_random_uuid()::TEXT, gen_random_uuid()::TEXT,
         'User attempted to access admin-only settings', NULL, false,
         NOW() - INTERVAL '6 hours'),

        -- Integration events
        (gen_random_uuid(), v_tenant_id,
         'Sync', 'Integration', NULL, 'SHOPIFY',
         'Synced 45 orders from Shopify', 1, -- Info
         NULL, 'System', NULL,
         NOW() - INTERVAL '30 minutes', NULL, 'SmartWMS-Scheduler/1.0',
         NULL,
         '{"OrdersSynced": 45, "ProductsSynced": 12, "Duration": "00:00:23"}',
         NULL,
         'Integrations', 'Shopify', gen_random_uuid()::TEXT, NULL,
         NULL, NULL, true,
         NOW() - INTERVAL '30 minutes'),

        (gen_random_uuid(), v_tenant_id,
         'SyncFailed', 'Integration', NULL, 'SAP-B1',
         'SAP sync failed due to connection timeout', 3, -- Error
         NULL, 'System', NULL,
         NOW() - INTERVAL '45 minutes', NULL, 'SmartWMS-Scheduler/1.0',
         NULL, NULL, NULL,
         'Integrations', 'SAP', gen_random_uuid()::TEXT, NULL,
         'Connection retry scheduled in 5 minutes', 'Connection timeout after 30 seconds', false,
         NOW() - INTERVAL '45 minutes');

        RAISE NOTICE 'Successfully inserted 15 audit logs';
    ELSE
        RAISE NOTICE 'Audit logs already exist, skipping seed';
    END IF;

    -- ============================================
    -- Activity Logs
    -- ============================================
    IF NOT EXISTS (SELECT 1 FROM "ActivityLogs" WHERE "TenantId" = v_tenant_id LIMIT 1) THEN
        INSERT INTO "ActivityLogs" (
            "Id", "TenantId",
            "ActivityType", "Description",
            "UserId", "UserName",
            "ActivityTime",
            "Module", "RelatedEntityId", "RelatedEntityType", "RelatedEntityNumber",
            "DeviceType", "DeviceId",
            "Notes",
            "CreatedAt"
        ) VALUES
        -- Picking activities
        (gen_random_uuid(), v_tenant_id,
         'PickStarted', 'Started picking task PICK-2024-0001',
         v_user_ids[3], 'Operator',
         NOW() - INTERVAL '2 hours',
         'Picking', NULL, 'PickTask', 'PICK-2024-0001',
         'Scanner', 'HC50-001',
         NULL,
         NOW() - INTERVAL '2 hours'),

        (gen_random_uuid(), v_tenant_id,
         'ItemPicked', 'Picked 10 units of PROD-001 from PICK-A-01',
         v_user_ids[3], 'Operator',
         NOW() - INTERVAL '1 hour' - INTERVAL '45 minutes',
         'Picking', v_product_ids[1], 'Product', 'PROD-001',
         'Scanner', 'HC50-001',
         'From location PICK-A-01',
         NOW() - INTERVAL '1 hour' - INTERVAL '45 minutes'),

        (gen_random_uuid(), v_tenant_id,
         'PickCompleted', 'Completed picking task PICK-2024-0001',
         v_user_ids[3], 'Operator',
         NOW() - INTERVAL '1 hour' - INTERVAL '30 minutes',
         'Picking', NULL, 'PickTask', 'PICK-2024-0001',
         'Scanner', 'HC50-001',
         'All items picked successfully',
         NOW() - INTERVAL '1 hour' - INTERVAL '30 minutes'),

        -- Packing activities
        (gen_random_uuid(), v_tenant_id,
         'PackingStarted', 'Started packing for order SO-2024-0005',
         v_user_ids[4], 'Packer',
         NOW() - INTERVAL '1 hour',
         'Packing', v_order_ids[1], 'SalesOrder', 'SO-2024-0005',
         'Web', NULL,
         NULL,
         NOW() - INTERVAL '1 hour'),

        (gen_random_uuid(), v_tenant_id,
         'PackageCreated', 'Created package PKG-2024-00010',
         v_user_ids[4], 'Packer',
         NOW() - INTERVAL '50 minutes',
         'Packing', NULL, 'Package', 'PKG-2024-00010',
         'Web', NULL,
         'Box dimensions: 40x30x20 cm',
         NOW() - INTERVAL '50 minutes'),

        (gen_random_uuid(), v_tenant_id,
         'LabelPrinted', 'Printed shipping label for PKG-2024-00010',
         v_user_ids[4], 'Packer',
         NOW() - INTERVAL '45 minutes',
         'Packing', NULL, 'Package', 'PKG-2024-00010',
         'Web', NULL,
         'DHL Express - JD014600006753906210',
         NOW() - INTERVAL '45 minutes'),

        -- Receiving activities
        (gen_random_uuid(), v_tenant_id,
         'ReceiptStarted', 'Started receiving GRN-2024-0010',
         v_user_ids[2], 'Warehouse Manager',
         NOW() - INTERVAL '4 hours',
         'Receiving', NULL, 'GoodsReceipt', 'GRN-2024-0010',
         'Web', NULL,
         NULL,
         NOW() - INTERVAL '4 hours'),

        (gen_random_uuid(), v_tenant_id,
         'ItemReceived', 'Received 100 units of PROD-002',
         v_user_ids[3], 'Operator',
         NOW() - INTERVAL '3 hours' - INTERVAL '30 minutes',
         'Receiving', v_product_ids[2], 'Product', 'PROD-002',
         'Scanner', 'HC50-002',
         'At dock RCV-DOCK-01',
         NOW() - INTERVAL '3 hours' - INTERVAL '30 minutes'),

        -- Putaway activities
        (gen_random_uuid(), v_tenant_id,
         'PutawayStarted', 'Started putaway task PUT-2024-0015',
         v_user_ids[3], 'Operator',
         NOW() - INTERVAL '3 hours',
         'Putaway', NULL, 'PutawayTask', 'PUT-2024-0015',
         'Scanner', 'HC50-001',
         NULL,
         NOW() - INTERVAL '3 hours'),

        (gen_random_uuid(), v_tenant_id,
         'ItemPutaway', 'Put away 100 units to BLK-A-01-01',
         v_user_ids[3], 'Operator',
         NOW() - INTERVAL '2 hours' - INTERVAL '30 minutes',
         'Putaway', v_product_ids[2], 'Product', 'PROD-002',
         'Scanner', 'HC50-001',
         'Batch: BATCH-2024-001',
         NOW() - INTERVAL '2 hours' - INTERVAL '30 minutes'),

        -- Cycle count activities
        (gen_random_uuid(), v_tenant_id,
         'CycleCountStarted', 'Started cycle count CC-2024-0002',
         v_user_ids[2], 'Warehouse Manager',
         NOW() - INTERVAL '1 day',
         'CycleCount', NULL, 'CycleCountSession', 'CC-2024-0002',
         'Mobile', 'TABLET-001',
         NULL,
         NOW() - INTERVAL '1 day'),

        (gen_random_uuid(), v_tenant_id,
         'LocationCounted', 'Counted location PICK-A-03',
         v_user_ids[3], 'Operator',
         NOW() - INTERVAL '23 hours',
         'CycleCount', NULL, 'Location', 'PICK-A-03',
         'Mobile', 'TABLET-002',
         'Expected: 75, Counted: 73 - Variance found',
         NOW() - INTERVAL '23 hours'),

        -- Search/view activities
        (gen_random_uuid(), v_tenant_id,
         'ViewedDashboard', 'Viewed warehouse dashboard',
         v_user_ids[1], 'Admin User',
         NOW() - INTERVAL '30 minutes',
         'Dashboard', NULL, NULL, NULL,
         'Web', NULL,
         NULL,
         NOW() - INTERVAL '30 minutes'),

        (gen_random_uuid(), v_tenant_id,
         'SearchPerformed', 'Searched for products matching "widget"',
         v_user_ids[2], 'Warehouse Manager',
         NOW() - INTERVAL '1 hour',
         'Inventory', NULL, 'Product', NULL,
         'Web', NULL,
         '12 results found',
         NOW() - INTERVAL '1 hour'),

        (gen_random_uuid(), v_tenant_id,
         'ReportGenerated', 'Generated Inventory Aging Report',
         v_user_ids[1], 'Admin User',
         NOW() - INTERVAL '2 hours',
         'Reports', NULL, 'Report', 'INVENTORY_AGING',
         'Web', NULL,
         'Date range: Last 30 days',
         NOW() - INTERVAL '2 hours');

        RAISE NOTICE 'Successfully inserted 15 activity logs';
    ELSE
        RAISE NOTICE 'Activity logs already exist, skipping seed';
    END IF;

    -- ============================================
    -- System Event Logs
    -- ============================================
    IF NOT EXISTS (SELECT 1 FROM "SystemEventLogs" WHERE "TenantId" = v_tenant_id LIMIT 1) THEN
        INSERT INTO "SystemEventLogs" (
            "Id", "TenantId",
            "EventType", "Category", "Severity",
            "Message", "Details",
            "EventTime",
            "Source", "SourceVersion", "MachineName",
            "ExceptionType", "ExceptionMessage", "StackTrace",
            "CorrelationId", "RequestId",
            "CreatedAt"
        ) VALUES
        -- Application events
        (gen_random_uuid(), v_tenant_id,
         'ApplicationStarted', 0, 2, -- Application, Information
         'Application started successfully',
         '{"Environment": "Production", "StartupTime": "00:00:12.345"}',
         NOW() - INTERVAL '1 day',
         'SmartWMS.API', '1.0.0', 'WMS-PROD-01',
         NULL, NULL, NULL,
         NULL, NULL,
         NOW() - INTERVAL '1 day'),

        (gen_random_uuid(), v_tenant_id,
         'HealthCheckPassed', 0, 2, -- Application, Information
         'All health checks passed',
         '{"Database": "Healthy", "Redis": "Healthy", "Integrations": "Healthy"}',
         NOW() - INTERVAL '5 minutes',
         'SmartWMS.API', '1.0.0', 'WMS-PROD-01',
         NULL, NULL, NULL,
         NULL, NULL,
         NOW() - INTERVAL '5 minutes'),

        -- Security events
        (gen_random_uuid(), v_tenant_id,
         'BruteForceDetected', 1, 3, -- Security, Warning
         'Potential brute force attack detected',
         '{"IpAddress": "10.0.0.55", "FailedAttempts": 5, "TimeWindow": "5 minutes"}',
         NOW() - INTERVAL '3 hours',
         'SmartWMS.Security', '1.0.0', 'WMS-PROD-01',
         NULL, NULL, NULL,
         gen_random_uuid()::TEXT, NULL,
         NOW() - INTERVAL '3 hours'),

        (gen_random_uuid(), v_tenant_id,
         'TokenRefreshed', 1, 2, -- Security, Information
         'User token refreshed',
         '{"UserId": "' || v_user_ids[1]::TEXT || '", "TokenLifetime": "7 days"}',
         NOW() - INTERVAL '2 hours',
         'SmartWMS.Auth', '1.0.0', 'WMS-PROD-01',
         NULL, NULL, NULL,
         gen_random_uuid()::TEXT, gen_random_uuid()::TEXT,
         NOW() - INTERVAL '2 hours'),

        -- Performance events
        (gen_random_uuid(), v_tenant_id,
         'SlowQueryDetected', 2, 3, -- Performance, Warning
         'Slow database query detected',
         '{"Query": "SELECT * FROM StockLevels...", "Duration": "2340ms", "RowsAffected": 15000}',
         NOW() - INTERVAL '4 hours',
         'SmartWMS.Database', '1.0.0', 'WMS-PROD-01',
         NULL, NULL, NULL,
         gen_random_uuid()::TEXT, gen_random_uuid()::TEXT,
         NOW() - INTERVAL '4 hours'),

        (gen_random_uuid(), v_tenant_id,
         'HighMemoryUsage', 2, 3, -- Performance, Warning
         'Memory usage above threshold',
         '{"CurrentUsage": "85%", "Threshold": "80%", "ProcessMemory": "2.1GB"}',
         NOW() - INTERVAL '6 hours',
         'SmartWMS.API', '1.0.0', 'WMS-PROD-01',
         NULL, NULL, NULL,
         NULL, NULL,
         NOW() - INTERVAL '6 hours'),

        -- Integration events
        (gen_random_uuid(), v_tenant_id,
         'IntegrationConnected', 3, 2, -- Integration, Information
         'Successfully connected to Shopify',
         '{"IntegrationId": "aaaabbbb-0001-0001-0001-000000000001", "Shop": "smartwms-demo"}',
         NOW() - INTERVAL '1 hour',
         'SmartWMS.Integrations', '1.0.0', 'WMS-PROD-01',
         NULL, NULL, NULL,
         gen_random_uuid()::TEXT, NULL,
         NOW() - INTERVAL '1 hour'),

        (gen_random_uuid(), v_tenant_id,
         'IntegrationFailed', 3, 4, -- Integration, Error
         'Failed to connect to Xero',
         '{"IntegrationId": "aaaabbbb-0001-0001-0004-000000000001", "RetryCount": 3}',
         NOW() - INTERVAL '2 days',
         'SmartWMS.Integrations', '1.0.0', 'WMS-PROD-01',
         'System.Net.Http.HttpRequestException', 'Token refresh failed: Invalid refresh token',
         'at SmartWMS.Integrations.XeroClient.RefreshTokenAsync()...',
         gen_random_uuid()::TEXT, NULL,
         NOW() - INTERVAL '2 days'),

        -- Scheduler events
        (gen_random_uuid(), v_tenant_id,
         'JobExecuted', 4, 2, -- Scheduler, Information
         'Daily inventory report job executed',
         '{"JobId": "daily-inventory-report", "Duration": "00:00:12.567", "NextRun": "' || (NOW() + INTERVAL '6 hours')::TEXT || '"}',
         NOW() - INTERVAL '18 hours',
         'SmartWMS.Scheduler', '1.0.0', 'WMS-PROD-01',
         NULL, NULL, NULL,
         gen_random_uuid()::TEXT, NULL,
         NOW() - INTERVAL '18 hours'),

        (gen_random_uuid(), v_tenant_id,
         'JobFailed', 4, 4, -- Scheduler, Error
         'Email notification job failed',
         '{"JobId": "email-notifications", "FailedItems": 3, "TotalItems": 50}',
         NOW() - INTERVAL '42 hours',
         'SmartWMS.Scheduler', '1.0.0', 'WMS-PROD-01',
         'System.Net.Mail.SmtpException', 'SMTP connection timeout',
         'at System.Net.Mail.SmtpClient.Send()...',
         gen_random_uuid()::TEXT, NULL,
         NOW() - INTERVAL '42 hours'),

        -- Database events
        (gen_random_uuid(), v_tenant_id,
         'MigrationApplied', 5, 2, -- Database, Information
         'Database migration applied successfully',
         '{"MigrationId": "20240115_AddCycleCountTables", "Duration": "00:00:05.234"}',
         NOW() - INTERVAL '7 days',
         'SmartWMS.Migrations', '1.0.0', 'WMS-PROD-01',
         NULL, NULL, NULL,
         NULL, NULL,
         NOW() - INTERVAL '7 days'),

        (gen_random_uuid(), v_tenant_id,
         'ConnectionPoolExhausted', 5, 5, -- Database, Critical
         'Database connection pool exhausted',
         '{"ActiveConnections": 100, "MaxPoolSize": 100, "WaitingRequests": 15}',
         NOW() - INTERVAL '12 hours',
         'SmartWMS.Database', '1.0.0', 'WMS-PROD-01',
         NULL, NULL, NULL,
         gen_random_uuid()::TEXT, NULL,
         NOW() - INTERVAL '12 hours'),

        -- Network events
        (gen_random_uuid(), v_tenant_id,
         'WebhookDelivered', 7, 2, -- Network, Information
         'Webhook delivered successfully',
         '{"Endpoint": "https://api.example.com/webhooks/orders", "StatusCode": 200, "Duration": "234ms"}',
         NOW() - INTERVAL '30 minutes',
         'SmartWMS.Webhooks', '1.0.0', 'WMS-PROD-01',
         NULL, NULL, NULL,
         gen_random_uuid()::TEXT, NULL,
         NOW() - INTERVAL '30 minutes'),

        (gen_random_uuid(), v_tenant_id,
         'WebhookFailed', 7, 4, -- Network, Error
         'Webhook delivery failed',
         '{"Endpoint": "https://api.example.com/webhooks/inventory", "StatusCode": 503, "RetryCount": 2}',
         NOW() - INTERVAL '2 hours',
         'SmartWMS.Webhooks', '1.0.0', 'WMS-PROD-01',
         'System.Net.Http.HttpRequestException', 'Service Unavailable',
         'at SmartWMS.Webhooks.WebhookSender.SendAsync()...',
         gen_random_uuid()::TEXT, NULL,
         NOW() - INTERVAL '2 hours');

        RAISE NOTICE 'Successfully inserted 14 system event logs';
    ELSE
        RAISE NOTICE 'System event logs already exist, skipping seed';
    END IF;

END $$;

-- Useful queries:
-- SELECT * FROM "AuditLogs" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46' ORDER BY "EventTime" DESC LIMIT 50;
-- SELECT * FROM "ActivityLogs" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46' ORDER BY "ActivityTime" DESC LIMIT 50;
-- SELECT * FROM "SystemEventLogs" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46' ORDER BY "EventTime" DESC LIMIT 50;
-- SELECT "EventType", COUNT(*) FROM "AuditLogs" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46' GROUP BY "EventType";
-- SELECT "Category", "Severity", COUNT(*) FROM "SystemEventLogs" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46' GROUP BY "Category", "Severity";
