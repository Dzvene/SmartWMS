-- ============================================
-- SmartWMS - Seed Notifications Data
-- PostgreSQL
-- ============================================
-- TenantId: e9006ab8-257f-4021-b60a-cbba785bad46
-- ============================================
-- NotificationType enum: 0=Info, 1=Success, 2=Warning, 3=Error, 4=Alert, 5=Task, 6=Reminder
-- NotificationPriority enum: 0=Low, 1=Normal, 2=High, 3=Urgent
-- ============================================

-- ============================================
-- Notification Templates (No TenantId - system-wide)
-- ============================================
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "NotificationTemplates" LIMIT 1) THEN
        INSERT INTO "NotificationTemplates" (
            "Id", "Code", "Name", "Description",
            "Type", "Category",
            "TitleTemplate", "MessageTemplate",
            "EmailSubjectTemplate", "EmailBodyTemplate",
            "PushTitleTemplate", "PushBodyTemplate",
            "IsActive", "CreatedAt"
        ) VALUES
        -- Inventory Alerts
        (gen_random_uuid(), 'LOW_STOCK', 'Low Stock Alert', 'Triggered when stock falls below reorder point',
         2, 'Inventory', -- Warning
         'Low Stock: {{ProductSku}}',
         'Product {{ProductSku}} ({{ProductName}}) has fallen to {{CurrentQty}} units, below the reorder point of {{ReorderPoint}}.',
         '[SmartWMS] Low Stock Alert - {{ProductSku}}',
         '<h2>Low Stock Alert</h2><p>Product <strong>{{ProductSku}}</strong> has fallen to <strong>{{CurrentQty}}</strong> units.</p>',
         'Low Stock: {{ProductSku}}', 'Stock at {{CurrentQty}} units',
         true, NOW()),

        (gen_random_uuid(), 'OUT_OF_STOCK', 'Out of Stock Alert', 'Triggered when stock reaches zero',
         3, 'Inventory', -- Error
         'Out of Stock: {{ProductSku}}',
         'Product {{ProductSku}} ({{ProductName}}) is now out of stock at location {{LocationCode}}.',
         '[SmartWMS] Out of Stock - {{ProductSku}}',
         '<h2>Out of Stock Alert</h2><p>Product <strong>{{ProductSku}}</strong> is now out of stock.</p>',
         'Out of Stock!', '{{ProductSku}} needs replenishment',
         true, NOW()),

        (gen_random_uuid(), 'EXPIRY_WARNING', 'Expiry Warning', 'Triggered when product is approaching expiry',
         2, 'Inventory', -- Warning
         'Expiring Soon: {{ProductSku}}',
         'Product {{ProductSku}} batch {{BatchNumber}} will expire on {{ExpiryDate}}. {{Quantity}} units affected.',
         '[SmartWMS] Expiry Warning - {{ProductSku}}',
         '<h2>Expiry Warning</h2><p>Batch {{BatchNumber}} of {{ProductSku}} expires on {{ExpiryDate}}.</p>',
         'Expiring Soon', '{{ProductSku}} expires {{ExpiryDate}}',
         true, NOW()),

        -- Order Notifications
        (gen_random_uuid(), 'ORDER_RECEIVED', 'Order Received', 'New sales order received',
         0, 'Orders', -- Info
         'New Order: {{OrderNumber}}',
         'Sales order {{OrderNumber}} has been received from {{CustomerName}}. Total lines: {{TotalLines}}.',
         '[SmartWMS] New Order Received - {{OrderNumber}}',
         '<h2>New Order Received</h2><p>Order <strong>{{OrderNumber}}</strong> from {{CustomerName}}.</p>',
         'New Order', '{{OrderNumber}} received',
         true, NOW()),

        (gen_random_uuid(), 'ORDER_SHIPPED', 'Order Shipped', 'Order has been shipped',
         1, 'Orders', -- Success
         'Order Shipped: {{OrderNumber}}',
         'Order {{OrderNumber}} has been shipped via {{CarrierName}}. Tracking: {{TrackingNumber}}.',
         '[SmartWMS] Order Shipped - {{OrderNumber}}',
         '<h2>Order Shipped</h2><p>Order {{OrderNumber}} is on its way. Tracking: {{TrackingNumber}}</p>',
         'Order Shipped', '{{OrderNumber}} is on its way',
         true, NOW()),

        (gen_random_uuid(), 'ORDER_DELAYED', 'Order Delayed', 'Order shipment is delayed',
         2, 'Orders', -- Warning
         'Order Delayed: {{OrderNumber}}',
         'Order {{OrderNumber}} shipment is delayed. Original date: {{OriginalDate}}, New estimated: {{NewDate}}.',
         '[SmartWMS] Order Delayed - {{OrderNumber}}',
         '<h2>Order Delayed</h2><p>Order {{OrderNumber}} has been delayed. We apologize for the inconvenience.</p>',
         'Order Delayed', '{{OrderNumber}} shipment delayed',
         true, NOW()),

        -- Task Notifications
        (gen_random_uuid(), 'TASK_ASSIGNED', 'Task Assigned', 'New task assigned to user',
         5, 'Tasks', -- Task
         'New Task: {{TaskType}}',
         'You have been assigned a new {{TaskType}} task ({{TaskNumber}}). Priority: {{Priority}}.',
         '[SmartWMS] New Task Assigned - {{TaskNumber}}',
         '<h2>New Task Assigned</h2><p>Task <strong>{{TaskNumber}}</strong> has been assigned to you.</p>',
         'New Task', '{{TaskType}} assigned',
         true, NOW()),

        (gen_random_uuid(), 'TASK_DUE_SOON', 'Task Due Soon', 'Task deadline approaching',
         6, 'Tasks', -- Reminder
         'Task Due Soon: {{TaskNumber}}',
         'Task {{TaskNumber}} is due in {{TimeRemaining}}. Please complete it soon.',
         '[SmartWMS] Task Due Soon - {{TaskNumber}}',
         '<h2>Task Due Soon</h2><p>Task {{TaskNumber}} needs your attention.</p>',
         'Due Soon', '{{TaskNumber}} due in {{TimeRemaining}}',
         true, NOW()),

        (gen_random_uuid(), 'TASK_OVERDUE', 'Task Overdue', 'Task is past due date',
         3, 'Tasks', -- Error
         'Overdue Task: {{TaskNumber}}',
         'Task {{TaskNumber}} is overdue by {{OverdueBy}}. Please complete it immediately.',
         '[SmartWMS] Task Overdue - {{TaskNumber}}',
         '<h2>Task Overdue</h2><p>Task {{TaskNumber}} was due on {{DueDate}}.</p>',
         'Task Overdue!', '{{TaskNumber}} needs immediate attention',
         true, NOW()),

        -- Receiving Notifications
        (gen_random_uuid(), 'RECEIPT_COMPLETE', 'Receipt Complete', 'Goods receipt completed',
         1, 'Receiving', -- Success
         'Receipt Complete: {{ReceiptNumber}}',
         'Goods receipt {{ReceiptNumber}} has been completed. {{TotalItems}} items received.',
         '[SmartWMS] Receipt Complete - {{ReceiptNumber}}',
         '<h2>Receipt Complete</h2><p>Receipt {{ReceiptNumber}} processed successfully.</p>',
         'Receipt Complete', '{{ReceiptNumber}} done',
         true, NOW()),

        (gen_random_uuid(), 'RECEIPT_DISCREPANCY', 'Receipt Discrepancy', 'Quantity discrepancy on receipt',
         2, 'Receiving', -- Warning
         'Receipt Discrepancy: {{ReceiptNumber}}',
         'Discrepancy found on receipt {{ReceiptNumber}}. Expected: {{Expected}}, Received: {{Actual}}.',
         '[SmartWMS] Receipt Discrepancy - {{ReceiptNumber}}',
         '<h2>Receipt Discrepancy</h2><p>Please verify receipt {{ReceiptNumber}}.</p>',
         'Discrepancy Found', '{{ReceiptNumber}} needs review',
         true, NOW()),

        -- Picking Notifications
        (gen_random_uuid(), 'PICK_WAVE_READY', 'Pick Wave Ready', 'Pick wave is ready to start',
         5, 'Picking', -- Task
         'Pick Wave Ready: {{WaveNumber}}',
         'Pick wave {{WaveNumber}} is ready with {{TotalPicks}} picks across {{TotalOrders}} orders.',
         '[SmartWMS] Pick Wave Ready - {{WaveNumber}}',
         '<h2>Pick Wave Ready</h2><p>Wave {{WaveNumber}} is ready for picking.</p>',
         'Wave Ready', '{{WaveNumber}} - {{TotalPicks}} picks',
         true, NOW()),

        (gen_random_uuid(), 'PICK_SHORTAGE', 'Pick Shortage', 'Unable to complete pick due to shortage',
         3, 'Picking', -- Error
         'Pick Shortage: {{OrderNumber}}',
         'Cannot complete pick for order {{OrderNumber}}. Product {{ProductSku}} has only {{Available}} units available.',
         '[SmartWMS] Pick Shortage - {{OrderNumber}}',
         '<h2>Pick Shortage</h2><p>Insufficient stock for order {{OrderNumber}}.</p>',
         'Pick Shortage', '{{ProductSku}} out of stock',
         true, NOW()),

        -- Return Notifications
        (gen_random_uuid(), 'RETURN_RECEIVED', 'Return Received', 'Return has been received',
         0, 'Returns', -- Info
         'Return Received: {{ReturnNumber}}',
         'Return {{ReturnNumber}} has been received. {{TotalItems}} items returned.',
         '[SmartWMS] Return Received - {{ReturnNumber}}',
         '<h2>Return Received</h2><p>Return {{ReturnNumber}} is ready for inspection.</p>',
         'Return Received', '{{ReturnNumber}} arrived',
         true, NOW()),

        (gen_random_uuid(), 'RETURN_INSPECTION_REQUIRED', 'Inspection Required', 'Return needs quality inspection',
         5, 'Returns', -- Task
         'Inspection Required: {{ReturnNumber}}',
         'Return {{ReturnNumber}} requires quality inspection. Reason: {{ReturnReason}}.',
         '[SmartWMS] Inspection Required - {{ReturnNumber}}',
         '<h2>Inspection Required</h2><p>Please inspect return {{ReturnNumber}}.</p>',
         'Inspection Needed', '{{ReturnNumber}} needs QC',
         true, NOW()),

        -- System Notifications
        (gen_random_uuid(), 'SYSTEM_MAINTENANCE', 'System Maintenance', 'Scheduled maintenance notification',
         4, 'System', -- Alert
         'Scheduled Maintenance',
         'System maintenance scheduled for {{MaintenanceDate}} from {{StartTime}} to {{EndTime}}. Please save your work.',
         '[SmartWMS] Scheduled Maintenance',
         '<h2>Scheduled Maintenance</h2><p>The system will be unavailable on {{MaintenanceDate}}.</p>',
         'Maintenance', 'System down {{MaintenanceDate}}',
         true, NOW()),

        (gen_random_uuid(), 'INTEGRATION_ERROR', 'Integration Error', 'External integration failed',
         3, 'System', -- Error
         'Integration Error: {{IntegrationName}}',
         'Integration {{IntegrationName}} encountered an error: {{ErrorMessage}}.',
         '[SmartWMS] Integration Error - {{IntegrationName}}',
         '<h2>Integration Error</h2><p>{{IntegrationName}} failed: {{ErrorMessage}}</p>',
         'Integration Error', '{{IntegrationName}} failed',
         true, NOW()),

        -- Cycle Count Notifications
        (gen_random_uuid(), 'CYCLE_COUNT_SCHEDULED', 'Cycle Count Scheduled', 'New cycle count scheduled',
         5, 'CycleCount', -- Task
         'Cycle Count Scheduled: {{CountNumber}}',
         'Cycle count {{CountNumber}} has been scheduled for {{ScheduledDate}}. {{TotalLocations}} locations to count.',
         '[SmartWMS] Cycle Count Scheduled',
         '<h2>Cycle Count Scheduled</h2><p>Count {{CountNumber}} on {{ScheduledDate}}.</p>',
         'Count Scheduled', '{{CountNumber}} on {{ScheduledDate}}',
         true, NOW()),

        (gen_random_uuid(), 'CYCLE_COUNT_VARIANCE', 'Variance Found', 'Cycle count found variance',
         2, 'CycleCount', -- Warning
         'Variance: {{CountNumber}}',
         'Cycle count {{CountNumber}} found {{VarianceCount}} variances requiring review.',
         '[SmartWMS] Variance Found - {{CountNumber}}',
         '<h2>Variance Found</h2><p>Review variances for count {{CountNumber}}.</p>',
         'Variance Found', '{{VarianceCount}} items need review',
         true, NOW());

        RAISE NOTICE 'Successfully inserted 20 notification templates';
    ELSE
        RAISE NOTICE 'Notification templates already exist, skipping seed';
    END IF;
END $$;

-- ============================================
-- Notification Preferences (Per User)
-- ============================================
DO $$
DECLARE
    v_tenant_id UUID := 'e9006ab8-257f-4021-b60a-cbba785bad46';
    v_user_ids UUID[];
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "NotificationPreferences" WHERE "TenantId" = v_tenant_id LIMIT 1) THEN
        -- Get users
        SELECT ARRAY_AGG("Id" ORDER BY "Email")
        INTO v_user_ids
        FROM (SELECT "Id", "Email" FROM "Users" WHERE "TenantId" = v_tenant_id AND "IsActive" = true ORDER BY "Email" LIMIT 5) u;

        IF v_user_ids IS NULL THEN
            RAISE NOTICE 'No users found, skipping notification preferences';
            RETURN;
        END IF;

        -- User 1 - Full notifications (Manager)
        INSERT INTO "NotificationPreferences" (
            "Id", "TenantId", "UserId", "Category",
            "InAppEnabled", "EmailEnabled", "PushEnabled",
            "MinimumPriority", "MuteAll", "MutedUntil", "CreatedAt"
        ) VALUES
        (gen_random_uuid(), v_tenant_id, v_user_ids[1], 'Inventory', true, true, true, 0, false, NULL, NOW()),
        (gen_random_uuid(), v_tenant_id, v_user_ids[1], 'Orders', true, true, true, 0, false, NULL, NOW()),
        (gen_random_uuid(), v_tenant_id, v_user_ids[1], 'Tasks', true, true, true, 0, false, NULL, NOW()),
        (gen_random_uuid(), v_tenant_id, v_user_ids[1], 'Receiving', true, true, false, 0, false, NULL, NOW()),
        (gen_random_uuid(), v_tenant_id, v_user_ids[1], 'Picking', true, true, false, 0, false, NULL, NOW()),
        (gen_random_uuid(), v_tenant_id, v_user_ids[1], 'Returns', true, true, false, 0, false, NULL, NOW()),
        (gen_random_uuid(), v_tenant_id, v_user_ids[1], 'System', true, true, true, 0, false, NULL, NOW()),
        (gen_random_uuid(), v_tenant_id, v_user_ids[1], 'CycleCount', true, true, false, 0, false, NULL, NOW());

        -- User 2 - Selective notifications (Warehouse Supervisor)
        INSERT INTO "NotificationPreferences" (
            "Id", "TenantId", "UserId", "Category",
            "InAppEnabled", "EmailEnabled", "PushEnabled",
            "MinimumPriority", "MuteAll", "CreatedAt"
        ) VALUES
        (gen_random_uuid(), v_tenant_id, v_user_ids[2], 'Inventory', true, true, true, 1, false, NOW()),
        (gen_random_uuid(), v_tenant_id, v_user_ids[2], 'Tasks', true, false, true, 1, false, NOW()),
        (gen_random_uuid(), v_tenant_id, v_user_ids[2], 'Receiving', true, false, true, 1, false, NOW()),
        (gen_random_uuid(), v_tenant_id, v_user_ids[2], 'Picking', true, false, true, 1, false, NOW()),
        (gen_random_uuid(), v_tenant_id, v_user_ids[2], 'System', true, true, false, 2, false, NOW());

        -- User 3 - Minimal (Operator)
        INSERT INTO "NotificationPreferences" (
            "Id", "TenantId", "UserId", "Category",
            "InAppEnabled", "EmailEnabled", "PushEnabled",
            "MinimumPriority", "MuteAll", "CreatedAt"
        ) VALUES
        (gen_random_uuid(), v_tenant_id, v_user_ids[3], 'Tasks', true, false, false, 1, false, NOW()),
        (gen_random_uuid(), v_tenant_id, v_user_ids[3], 'Picking', true, false, false, 1, false, NOW()),
        (gen_random_uuid(), v_tenant_id, v_user_ids[3], 'System', true, false, false, 2, false, NOW());

        -- User 4 - Muted temporarily
        INSERT INTO "NotificationPreferences" (
            "Id", "TenantId", "UserId", "Category",
            "InAppEnabled", "EmailEnabled", "PushEnabled",
            "MinimumPriority", "MuteAll", "MutedUntil", "CreatedAt"
        ) VALUES
        (gen_random_uuid(), v_tenant_id, v_user_ids[4], 'Inventory', true, false, false, 1, true, NOW() + INTERVAL '2 hours', NOW()),
        (gen_random_uuid(), v_tenant_id, v_user_ids[4], 'Tasks', true, false, false, 1, true, NOW() + INTERVAL '2 hours', NOW());

        RAISE NOTICE 'Successfully inserted notification preferences for users';
    ELSE
        RAISE NOTICE 'Notification preferences already exist, skipping seed';
    END IF;
END $$;

-- ============================================
-- Notifications (Actual Notifications)
-- ============================================
DO $$
DECLARE
    v_tenant_id UUID := 'e9006ab8-257f-4021-b60a-cbba785bad46';
    v_user_ids UUID[];
    v_product_ids UUID[];
    v_order_ids UUID[];
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "Notifications" WHERE "TenantId" = v_tenant_id LIMIT 1) THEN
        -- Get users
        SELECT ARRAY_AGG("Id" ORDER BY "Email")
        INTO v_user_ids
        FROM (SELECT "Id", "Email" FROM "Users" WHERE "TenantId" = v_tenant_id AND "IsActive" = true ORDER BY "Email" LIMIT 5) u;

        -- Get products
        SELECT ARRAY_AGG("Id" ORDER BY "Sku")
        INTO v_product_ids
        FROM (SELECT "Id", "Sku" FROM "Products" WHERE "TenantId" = v_tenant_id AND "IsActive" = true ORDER BY "Sku" LIMIT 5) p;

        -- Get sales orders
        SELECT ARRAY_AGG("Id" ORDER BY "OrderNumber")
        INTO v_order_ids
        FROM (SELECT "Id", "OrderNumber" FROM "SalesOrders" WHERE "TenantId" = v_tenant_id ORDER BY "OrderNumber" LIMIT 5) so;

        IF v_user_ids IS NULL THEN
            RAISE NOTICE 'No users found, skipping notifications';
            RETURN;
        END IF;

        INSERT INTO "Notifications" (
            "Id", "TenantId", "UserId", "Type", "Priority",
            "Title", "Message", "Category",
            "EntityType", "EntityId", "ActionUrl",
            "IsRead", "ReadAt", "IsArchived", "ArchivedAt",
            "EmailSent", "EmailSentAt", "PushSent", "PushSentAt",
            "ExpiresAt", "CreatedAt"
        ) VALUES
        -- Read notifications (archived)
        (gen_random_uuid(), v_tenant_id, v_user_ids[1], 1, 1, -- Success, Normal
         'Order Shipped: SO-2024-0001', 'Order SO-2024-0001 has been shipped via DHL. Tracking: JD123456789.',
         'Orders', 'SalesOrder', v_order_ids[1], '/orders/sales/' || v_order_ids[1],
         true, NOW() - INTERVAL '3 days', true, NOW() - INTERVAL '2 days',
         true, NOW() - INTERVAL '3 days', false, NULL,
         NULL, NOW() - INTERVAL '3 days'),

        (gen_random_uuid(), v_tenant_id, v_user_ids[1], 1, 1,
         'Receipt Complete: GRN-2024-0001', 'Goods receipt GRN-2024-0001 completed. 150 items received.',
         'Receiving', 'GoodsReceipt', NULL, '/receiving/goods-receipts',
         true, NOW() - INTERVAL '2 days', true, NOW() - INTERVAL '1 day',
         true, NOW() - INTERVAL '2 days', false, NULL,
         NULL, NOW() - INTERVAL '2 days'),

        -- Read but not archived
        (gen_random_uuid(), v_tenant_id, v_user_ids[1], 2, 2, -- Warning, High
         'Low Stock: PROD-001', 'Product PROD-001 has fallen to 25 units, below reorder point of 50.',
         'Inventory', 'Product', v_product_ids[1], '/inventory/stock-levels',
         true, NOW() - INTERVAL '1 day', false, NULL,
         true, NOW() - INTERVAL '1 day', true, NOW() - INTERVAL '1 day',
         NOW() + INTERVAL '7 days', NOW() - INTERVAL '1 day'),

        (gen_random_uuid(), v_tenant_id, v_user_ids[2], 0, 1, -- Info, Normal
         'New Order: SO-2024-0010', 'Sales order SO-2024-0010 received from Customer A.',
         'Orders', 'SalesOrder', v_order_ids[2], '/orders/sales/' || v_order_ids[2],
         true, NOW() - INTERVAL '12 hours', false, NULL,
         true, NOW() - INTERVAL '12 hours', false, NULL,
         NULL, NOW() - INTERVAL '12 hours'),

        -- Unread notifications
        (gen_random_uuid(), v_tenant_id, v_user_ids[1], 5, 2, -- Task, High
         'Task Assigned: Putaway', 'You have been assigned a new Putaway task (PUT-2024-0023). Priority: High.',
         'Tasks', 'PutawayTask', NULL, '/operations/tasks',
         false, NULL, false, NULL,
         false, NULL, false, NULL,
         NOW() + INTERVAL '3 days', NOW() - INTERVAL '30 minutes'),

        (gen_random_uuid(), v_tenant_id, v_user_ids[1], 3, 3, -- Error, Urgent
         'Out of Stock: PROD-005', 'Product PROD-005 is now out of stock at location PICK-A-01.',
         'Inventory', 'Product', v_product_ids[5], '/inventory/stock-levels',
         false, NULL, false, NULL,
         false, NULL, true, NOW() - INTERVAL '15 minutes',
         NOW() + INTERVAL '24 hours', NOW() - INTERVAL '15 minutes'),

        (gen_random_uuid(), v_tenant_id, v_user_ids[1], 2, 2, -- Warning, High
         'Expiring Soon: Batch B2024-001', 'Product PROD-003 batch B2024-001 will expire on 2024-02-15. 100 units affected.',
         'Inventory', 'Product', v_product_ids[3], '/inventory/stock-levels',
         false, NULL, false, NULL,
         true, NOW() - INTERVAL '2 hours', false, NULL,
         NOW() + INTERVAL '7 days', NOW() - INTERVAL '2 hours'),

        (gen_random_uuid(), v_tenant_id, v_user_ids[2], 5, 2, -- Task, High
         'Pick Wave Ready: WAVE-2024-015', 'Pick wave WAVE-2024-015 is ready with 45 picks across 12 orders.',
         'Picking', 'FulfillmentBatch', NULL, '/outbound/picking',
         false, NULL, false, NULL,
         false, NULL, true, NOW() - INTERVAL '10 minutes',
         NOW() + INTERVAL '8 hours', NOW() - INTERVAL '10 minutes'),

        (gen_random_uuid(), v_tenant_id, v_user_ids[3], 5, 1, -- Task, Normal
         'Task Assigned: Picking', 'You have been assigned picking task for WAVE-2024-015.',
         'Tasks', 'PickTask', NULL, '/operations/tasks',
         false, NULL, false, NULL,
         false, NULL, false, NULL,
         NOW() + INTERVAL '4 hours', NOW() - INTERVAL '8 minutes'),

        (gen_random_uuid(), v_tenant_id, v_user_ids[3], 6, 1, -- Reminder, Normal
         'Task Due Soon: PICK-2024-0050', 'Task PICK-2024-0050 is due in 2 hours. Please complete it soon.',
         'Tasks', 'PickTask', NULL, '/operations/tasks',
         false, NULL, false, NULL,
         false, NULL, false, NULL,
         NOW() + INTERVAL '2 hours', NOW() - INTERVAL '5 minutes'),

        -- Cycle count notification
        (gen_random_uuid(), v_tenant_id, v_user_ids[1], 2, 2, -- Warning, High
         'Variance: CC-2024-0004', 'Cycle count CC-2024-0004 found 2 variances requiring review.',
         'CycleCount', 'CycleCountSession', NULL, '/inventory/cycle-count',
         false, NULL, false, NULL,
         true, NOW() - INTERVAL '1 hour', false, NULL,
         NOW() + INTERVAL '3 days', NOW() - INTERVAL '1 hour'),

        -- Return notification
        (gen_random_uuid(), v_tenant_id, v_user_ids[2], 5, 1, -- Task, Normal
         'Inspection Required: RET-2024-0003', 'Return RET-2024-0003 requires quality inspection. Reason: Damaged during shipping.',
         'Returns', 'ReturnOrder', NULL, '/inbound/returns',
         false, NULL, false, NULL,
         false, NULL, false, NULL,
         NOW() + INTERVAL '2 days', NOW() - INTERVAL '45 minutes'),

        -- Integration error (for admin)
        (gen_random_uuid(), v_tenant_id, v_user_ids[1], 3, 3, -- Error, Urgent
         'Integration Error: Shopify', 'Integration Shopify encountered an error: Connection timeout after 30 seconds.',
         'System', 'Integration', NULL, '/config/integrations',
         false, NULL, false, NULL,
         true, NOW() - INTERVAL '20 minutes', true, NOW() - INTERVAL '20 minutes',
         NOW() + INTERVAL '1 day', NOW() - INTERVAL '20 minutes'),

        -- System maintenance (for all)
        (gen_random_uuid(), v_tenant_id, v_user_ids[1], 4, 2, -- Alert, High
         'Scheduled Maintenance', 'System maintenance scheduled for 2024-02-01 from 02:00 to 04:00 UTC. Please save your work.',
         'System', NULL, NULL, NULL,
         false, NULL, false, NULL,
         true, NOW() - INTERVAL '5 minutes', false, NULL,
         NOW() + INTERVAL '7 days', NOW() - INTERVAL '5 minutes'),

        (gen_random_uuid(), v_tenant_id, v_user_ids[2], 4, 2,
         'Scheduled Maintenance', 'System maintenance scheduled for 2024-02-01 from 02:00 to 04:00 UTC. Please save your work.',
         'System', NULL, NULL, NULL,
         false, NULL, false, NULL,
         true, NOW() - INTERVAL '5 minutes', false, NULL,
         NOW() + INTERVAL '7 days', NOW() - INTERVAL '5 minutes'),

        (gen_random_uuid(), v_tenant_id, v_user_ids[3], 4, 2,
         'Scheduled Maintenance', 'System maintenance scheduled for 2024-02-01 from 02:00 to 04:00 UTC. Please save your work.',
         'System', NULL, NULL, NULL,
         false, NULL, false, NULL,
         false, NULL, false, NULL,
         NOW() + INTERVAL '7 days', NOW() - INTERVAL '5 minutes');

        RAISE NOTICE 'Successfully inserted 17 notifications';
    ELSE
        RAISE NOTICE 'Notifications already exist, skipping seed';
    END IF;
END $$;

-- Useful queries:
-- SELECT * FROM "NotificationTemplates";
-- SELECT * FROM "NotificationPreferences" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
-- SELECT * FROM "Notifications" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46' ORDER BY "CreatedAt" DESC;
-- SELECT n.*, u."Email" FROM "Notifications" n JOIN "Users" u ON n."UserId" = u."Id" WHERE n."IsRead" = false;
