-- ============================================
-- SmartWMS - Seed Automation Data
-- PostgreSQL
-- ============================================
-- TenantId: e9006ab8-257f-4021-b60a-cbba785bad46
-- ============================================
-- TriggerType enum: 0=EntityCreated, 1=EntityUpdated, 2=EntityDeleted, 3=StatusChanged, 4=ThresholdCrossed, 5=Schedule, 6=Manual, 7=WebhookReceived
-- ActionType enum: 0=CreateTask, 1=AssignTask, 2=SendNotification, 3=SendEmail, 4=SendWebhook, 5=UpdateEntityStatus, 6=UpdateEntityField, 7=GenerateReport, 8=TriggerSync, 9=CreateAdjustment, 10=CreateTransfer, 11=ExecuteScript
-- ConditionOperator enum: 0=Equals, 1=NotEquals, 2=GreaterThan, 3=GreaterThanOrEquals, 4=LessThan, 5=LessThanOrEquals, 6=Contains, 7=NotContains, 8=StartsWith, 9=EndsWith, 10=IsNull, 11=IsNotNull, 12=In, 13=NotIn
-- ConditionLogic enum: 0=And, 1=Or
-- ExecutionStatus enum: 0=Pending, 1=Running, 2=Completed, 3=Failed, 4=Skipped, 5=Cancelled
-- ScheduledJobStatus enum: 0=Pending, 1=Running, 2=Completed, 3=Failed, 4=Cancelled, 5=Retrying
-- ============================================

DO $$
DECLARE
    v_tenant_id UUID := 'e9006ab8-257f-4021-b60a-cbba785bad46';
    v_rule_low_stock UUID := 'cccc0001-0001-0001-0001-000000000001';
    v_rule_order_created UUID := 'cccc0001-0001-0001-0002-000000000001';
    v_rule_daily_report UUID := 'cccc0001-0001-0001-0003-000000000001';
    v_rule_expiry_check UUID := 'cccc0001-0001-0001-0004-000000000001';
    v_rule_priority_order UUID := 'cccc0001-0001-0001-0005-000000000001';
    v_rule_auto_putaway UUID := 'cccc0001-0001-0001-0006-000000000001';
    v_rule_sync_shopify UUID := 'cccc0001-0001-0001-0007-000000000001';
    v_product_ids UUID[];
    v_order_ids UUID[];
BEGIN
    -- ============================================
    -- Action Templates (Reusable action configurations)
    -- ============================================
    IF NOT EXISTS (SELECT 1 FROM "AutomationActionTemplates" WHERE "TenantId" = v_tenant_id LIMIT 1) THEN
        INSERT INTO "AutomationActionTemplates" (
            "Id", "TenantId", "Code", "Name", "Description",
            "ActionType", "ConfigJson",
            "IsSystem", "IsActive", "CreatedAt"
        ) VALUES
        -- Notification templates
        (gen_random_uuid(), v_tenant_id, 'NOTIFY_ADMIN_URGENT', 'Notify Admins - Urgent', 'Send urgent notification to all administrators',
         2, '{"Priority": "Urgent", "SendToAllAdmins": true, "NotificationType": "Alert"}', -- SendNotification
         true, true, NOW()),

        (gen_random_uuid(), v_tenant_id, 'NOTIFY_WAREHOUSE_MANAGER', 'Notify Warehouse Manager', 'Send notification to warehouse managers',
         2, '{"Priority": "High", "RoleIds": [], "NotificationType": "Warning"}', -- SendNotification
         true, true, NOW()),

        (gen_random_uuid(), v_tenant_id, 'NOTIFY_INVENTORY_TEAM', 'Notify Inventory Team', 'Send notification to inventory management team',
         2, '{"Priority": "Normal", "RoleIds": [], "NotificationType": "Info"}', -- SendNotification
         false, true, NOW()),

        -- Email templates
        (gen_random_uuid(), v_tenant_id, 'EMAIL_LOW_STOCK_ALERT', 'Low Stock Email Alert', 'Send email alert for low stock items',
         3, '{"TemplateCode": "LOW_STOCK", "ToRoleIds": [], "IsHtml": true}', -- SendEmail
         true, true, NOW()),

        (gen_random_uuid(), v_tenant_id, 'EMAIL_DAILY_SUMMARY', 'Daily Summary Email', 'Send daily operations summary email',
         3, '{"TemplateCode": "DAILY_SUMMARY", "ToRoleIds": [], "IsHtml": true}', -- SendEmail
         true, true, NOW()),

        -- Task templates
        (gen_random_uuid(), v_tenant_id, 'CREATE_PUTAWAY_TASK', 'Create Putaway Task', 'Automatically create putaway task',
         0, '{"TaskType": "Putaway", "Priority": 5, "Notes": "Auto-generated putaway task"}', -- CreateTask
         true, true, NOW()),

        (gen_random_uuid(), v_tenant_id, 'CREATE_PICK_TASK', 'Create Pick Task', 'Automatically create pick task',
         0, '{"TaskType": "Pick", "Priority": 5, "Notes": "Auto-generated pick task"}', -- CreateTask
         true, true, NOW()),

        (gen_random_uuid(), v_tenant_id, 'CREATE_CYCLE_COUNT', 'Create Cycle Count', 'Automatically create cycle count task',
         0, '{"TaskType": "CycleCount", "Priority": 3, "Notes": "Scheduled cycle count"}', -- CreateTask
         true, true, NOW()),

        -- Webhook templates
        (gen_random_uuid(), v_tenant_id, 'WEBHOOK_ORDER_STATUS', 'Order Status Webhook', 'Send order status update to external system',
         4, '{"Url": "https://api.example.com/webhooks/orders", "Method": "POST", "AuthType": "bearer", "TimeoutSeconds": 30}', -- SendWebhook
         false, true, NOW()),

        -- Integration templates
        (gen_random_uuid(), v_tenant_id, 'SYNC_INVENTORY', 'Sync Inventory', 'Trigger inventory sync with integration',
         8, '{"EntityType": "StockLevel", "Direction": "Outbound"}', -- TriggerSync
         true, true, NOW());

        RAISE NOTICE 'Successfully inserted 10 action templates';
    END IF;

    -- ============================================
    -- Automation Rules
    -- ============================================
    IF NOT EXISTS (SELECT 1 FROM "AutomationRules" WHERE "TenantId" = v_tenant_id LIMIT 1) THEN
        INSERT INTO "AutomationRules" (
            "Id", "TenantId", "Name", "Description",
            "TriggerType", "TriggerEntityType", "TriggerEvent",
            "CronExpression", "Timezone",
            "ConditionsJson",
            "ActionType", "ActionConfigJson",
            "IsActive", "Priority",
            "MaxExecutionsPerDay", "CooldownSeconds",
            "TotalExecutions", "SuccessfulExecutions", "FailedExecutions",
            "LastExecutedAt", "NextScheduledAt",
            "CreatedAt"
        ) VALUES
        -- Rule 1: Low Stock Alert
        (v_rule_low_stock, v_tenant_id, 'Low Stock Alert', 'Notify when stock falls below reorder point',
         4, 'StockLevel', 'QuantityChanged', -- ThresholdCrossed
         NULL, NULL,
         '[{"field": "AvailableQuantity", "operator": "LessThan", "value": "ReorderPoint", "valueType": "Field"}]',
         2, '{"TemplateCode": "LOW_STOCK", "Priority": "High", "NotifyCreator": false, "RoleIds": [], "ActionUrl": "/inventory/stock-levels"}', -- SendNotification
         true, 10,
         100, 300,
         45, 43, 2,
         NOW() - INTERVAL '2 hours', NULL,
         NOW()),

        -- Rule 2: New Order Notification
        (v_rule_order_created, v_tenant_id, 'New Order Notification', 'Notify warehouse when new sales order is created',
         0, 'SalesOrder', 'Created', -- EntityCreated
         NULL, NULL,
         NULL,
         2, '{"Title": "New Sales Order", "Message": "Order {{OrderNumber}} received from {{CustomerName}}", "Priority": "Normal", "RoleIds": []}', -- SendNotification
         true, 50,
         NULL, 60,
         128, 128, 0,
         NOW() - INTERVAL '30 minutes', NULL,
         NOW()),

        -- Rule 3: Daily Inventory Report
        (v_rule_daily_report, v_tenant_id, 'Daily Inventory Report', 'Generate and email daily inventory summary',
         5, NULL, NULL, -- Schedule
         '0 6 * * *', 'Europe/Stockholm', -- Every day at 6:00 AM
         NULL,
         7, '{"ReportType": "DailyInventorySummary", "EmailReport": true, "EmailAddresses": ["warehouse@example.com"]}', -- GenerateReport
         true, 100,
         1, NULL,
         30, 29, 1,
         NOW() - INTERVAL '18 hours', NOW() + INTERVAL '6 hours',
         NOW()),

        -- Rule 4: Expiry Check
        (v_rule_expiry_check, v_tenant_id, 'Expiring Stock Check', 'Check for products expiring within 30 days',
         5, NULL, NULL, -- Schedule
         '0 7 * * 1', 'Europe/Stockholm', -- Every Monday at 7:00 AM
         NULL,
         2, '{"TemplateCode": "EXPIRY_WARNING", "Priority": "High", "RoleIds": []}', -- SendNotification
         true, 80,
         1, NULL,
         4, 4, 0,
         NOW() - INTERVAL '3 days', NOW() + INTERVAL '4 days',
         NOW()),

        -- Rule 5: Priority Order Fast-Track
        (v_rule_priority_order, v_tenant_id, 'Priority Order Fast-Track', 'Auto-assign priority orders to senior pickers',
         3, 'SalesOrder', 'StatusChanged', -- StatusChanged
         NULL, NULL,
         '[{"field": "Priority", "operator": "GreaterThanOrEquals", "value": "8", "valueType": "Number"}, {"field": "Status", "operator": "Equals", "value": "Confirmed", "valueType": "String"}]',
         1, '{"Priority": 10, "AssignToRoleId": null, "Notes": "Fast-tracked due to high priority"}', -- AssignTask
         true, 20,
         50, 0,
         23, 21, 2,
         NOW() - INTERVAL '4 hours', NULL,
         NOW()),

        -- Rule 6: Auto Putaway After Receiving
        (v_rule_auto_putaway, v_tenant_id, 'Auto Create Putaway', 'Create putaway task when goods are received',
         3, 'GoodsReceipt', 'StatusChanged', -- StatusChanged
         NULL, NULL,
         '[{"field": "Status", "operator": "Equals", "value": "Complete", "valueType": "String"}]',
         0, '{"TaskType": "Putaway", "Priority": 5, "Notes": "Auto-generated from GRN"}', -- CreateTask
         true, 30,
         NULL, 0,
         67, 65, 2,
         NOW() - INTERVAL '1 day', NULL,
         NOW()),

        -- Rule 7: Sync to Shopify on Stock Change
        (v_rule_sync_shopify, v_tenant_id, 'Sync Stock to Shopify', 'Sync inventory levels to Shopify when stock changes',
         1, 'StockLevel', 'Updated', -- EntityUpdated
         NULL, NULL,
         '[{"field": "AvailableQuantity", "operator": "IsNotNull", "value": "", "valueType": "String"}]',
         8, '{"IntegrationId": "aaaabbbb-0001-0001-0001-000000000001", "EntityType": "StockLevel", "Direction": "Outbound"}', -- TriggerSync
         true, 40,
         1000, 5,
         456, 450, 6,
         NOW() - INTERVAL '10 minutes', NULL,
         NOW()),

        -- Rule 8: Weekly Cycle Count (Inactive)
        (gen_random_uuid(), v_tenant_id, 'Weekly Cycle Count - Zone A', 'Schedule weekly cycle count for Zone A',
         5, NULL, NULL, -- Schedule
         '0 5 * * 0', 'Europe/Stockholm', -- Every Sunday at 5:00 AM
         NULL,
         0, '{"TaskType": "CycleCount", "Priority": 3, "Notes": "Scheduled weekly count"}', -- CreateTask
         false, 100, -- Inactive
         1, NULL,
         0, 0, 0,
         NULL, NULL,
         NOW());

        RAISE NOTICE 'Successfully inserted 8 automation rules';

        -- ============================================
        -- Rule Conditions (for rules that have them)
        -- ============================================
        INSERT INTO "AutomationRuleConditions" (
            "Id", "TenantId", "RuleId",
            "Order", "Logic", "Field", "Operator", "Value", "ValueType",
            "CreatedAt"
        ) VALUES
        -- Low Stock Rule Conditions
        (gen_random_uuid(), v_tenant_id, v_rule_low_stock,
         1, 0, 'AvailableQuantity', 4, 'ReorderPoint', 'Field', NOW()), -- AND, LessThan

        -- Priority Order Conditions
        (gen_random_uuid(), v_tenant_id, v_rule_priority_order,
         1, 0, 'Priority', 3, '8', 'Number', NOW()), -- AND, GreaterThanOrEquals
        (gen_random_uuid(), v_tenant_id, v_rule_priority_order,
         2, 0, 'Status', 0, 'Confirmed', 'String', NOW()), -- AND, Equals

        -- Auto Putaway Conditions
        (gen_random_uuid(), v_tenant_id, v_rule_auto_putaway,
         1, 0, 'Status', 0, 'Complete', 'String', NOW()), -- AND, Equals

        -- Sync Shopify Conditions
        (gen_random_uuid(), v_tenant_id, v_rule_sync_shopify,
         1, 0, 'AvailableQuantity', 11, '', 'String', NOW()); -- AND, IsNotNull

        RAISE NOTICE 'Successfully inserted rule conditions';

        -- ============================================
        -- Rule Executions (History)
        -- ============================================
        -- Get some sample data for execution logs
        SELECT ARRAY_AGG("Id" ORDER BY "Sku")
        INTO v_product_ids
        FROM (SELECT "Id", "Sku" FROM "Products" WHERE "TenantId" = v_tenant_id AND "IsActive" = true ORDER BY "Sku" LIMIT 5) p;

        SELECT ARRAY_AGG("Id" ORDER BY "OrderNumber")
        INTO v_order_ids
        FROM (SELECT "Id", "OrderNumber" FROM "SalesOrders" WHERE "TenantId" = v_tenant_id ORDER BY "OrderNumber" LIMIT 5) so;

        INSERT INTO "AutomationRuleExecutions" (
            "Id", "TenantId", "RuleId",
            "TriggerEntityType", "TriggerEntityId", "TriggerEventData",
            "StartedAt", "CompletedAt", "DurationMs",
            "Status", "ConditionsMet", "ResultData", "ErrorMessage",
            "CreatedEntityType", "CreatedEntityId",
            "CreatedAt"
        ) VALUES
        -- Low Stock executions
        (gen_random_uuid(), v_tenant_id, v_rule_low_stock,
         'StockLevel', NULL, '{"ProductSku": "PROD-001", "CurrentQty": 25, "ReorderPoint": 50}',
         NOW() - INTERVAL '2 hours', NOW() - INTERVAL '2 hours' + INTERVAL '500 milliseconds', 456,
         2, true, '{"NotificationId": "xxx", "SentTo": 3}', NULL, -- Completed
         'Notification', NULL,
         NOW() - INTERVAL '2 hours'),

        (gen_random_uuid(), v_tenant_id, v_rule_low_stock,
         'StockLevel', NULL, '{"ProductSku": "PROD-003", "CurrentQty": 10, "ReorderPoint": 25}',
         NOW() - INTERVAL '5 hours', NOW() - INTERVAL '5 hours' + INTERVAL '380 milliseconds', 380,
         2, true, '{"NotificationId": "xxx", "SentTo": 3}', NULL, -- Completed
         'Notification', NULL,
         NOW() - INTERVAL '5 hours'),

        (gen_random_uuid(), v_tenant_id, v_rule_low_stock,
         'StockLevel', NULL, '{"ProductSku": "PROD-007", "CurrentQty": 100, "ReorderPoint": 50}',
         NOW() - INTERVAL '6 hours', NOW() - INTERVAL '6 hours' + INTERVAL '45 milliseconds', 45,
         4, false, NULL, NULL, -- Skipped (conditions not met)
         NULL, NULL,
         NOW() - INTERVAL '6 hours'),

        -- Order Created executions
        (gen_random_uuid(), v_tenant_id, v_rule_order_created,
         'SalesOrder', v_order_ids[1], '{"OrderNumber": "SO-2024-0001", "CustomerName": "Acme Corp"}',
         NOW() - INTERVAL '30 minutes', NOW() - INTERVAL '30 minutes' + INTERVAL '234 milliseconds', 234,
         2, true, '{"NotificationId": "xxx", "SentTo": 5}', NULL, -- Completed
         'Notification', NULL,
         NOW() - INTERVAL '30 minutes'),

        (gen_random_uuid(), v_tenant_id, v_rule_order_created,
         'SalesOrder', v_order_ids[2], '{"OrderNumber": "SO-2024-0002", "CustomerName": "Tech Solutions"}',
         NOW() - INTERVAL '2 hours', NOW() - INTERVAL '2 hours' + INTERVAL '198 milliseconds', 198,
         2, true, '{"NotificationId": "xxx", "SentTo": 5}', NULL, -- Completed
         'Notification', NULL,
         NOW() - INTERVAL '2 hours'),

        -- Daily Report executions
        (gen_random_uuid(), v_tenant_id, v_rule_daily_report,
         NULL, NULL, '{"ScheduledAt": "2024-01-15T06:00:00Z"}',
         NOW() - INTERVAL '18 hours', NOW() - INTERVAL '18 hours' + INTERVAL '12340 milliseconds', 12340,
         2, true, '{"ReportGenerated": true, "EmailSent": true, "Recipients": 2}', NULL, -- Completed
         NULL, NULL,
         NOW() - INTERVAL '18 hours'),

        (gen_random_uuid(), v_tenant_id, v_rule_daily_report,
         NULL, NULL, '{"ScheduledAt": "2024-01-14T06:00:00Z"}',
         NOW() - INTERVAL '42 hours', NOW() - INTERVAL '42 hours' + INTERVAL '15670 milliseconds', 15670,
         3, true, NULL, 'Email delivery failed: SMTP connection timeout', -- Failed
         NULL, NULL,
         NOW() - INTERVAL '42 hours'),

        -- Sync Shopify executions (recent)
        (gen_random_uuid(), v_tenant_id, v_rule_sync_shopify,
         'StockLevel', NULL, '{"ProductSku": "PROD-001", "NewQuantity": 150}',
         NOW() - INTERVAL '10 minutes', NOW() - INTERVAL '10 minutes' + INTERVAL '890 milliseconds', 890,
         2, true, '{"SyncedToShopify": true, "InventoryItemId": "12345"}', NULL, -- Completed
         NULL, NULL,
         NOW() - INTERVAL '10 minutes'),

        (gen_random_uuid(), v_tenant_id, v_rule_sync_shopify,
         'StockLevel', NULL, '{"ProductSku": "PROD-002", "NewQuantity": 75}',
         NOW() - INTERVAL '15 minutes', NOW() - INTERVAL '15 minutes' + INTERVAL '1234 milliseconds', 1234,
         2, true, '{"SyncedToShopify": true, "InventoryItemId": "12346"}', NULL, -- Completed
         NULL, NULL,
         NOW() - INTERVAL '15 minutes'),

        -- Auto Putaway execution
        (gen_random_uuid(), v_tenant_id, v_rule_auto_putaway,
         'GoodsReceipt', NULL, '{"ReceiptNumber": "GRN-2024-0050", "Status": "Complete"}',
         NOW() - INTERVAL '1 day', NOW() - INTERVAL '1 day' + INTERVAL '567 milliseconds', 567,
         2, true, '{"TaskCreated": true, "TaskNumber": "PUT-2024-0123"}', NULL, -- Completed
         'PutawayTask', NULL,
         NOW() - INTERVAL '1 day');

        RAISE NOTICE 'Successfully inserted 11 rule executions';

        -- ============================================
        -- Scheduled Jobs
        -- ============================================
        INSERT INTO "AutomationScheduledJobs" (
            "Id", "TenantId", "RuleId",
            "ScheduledFor", "StartedAt", "CompletedAt",
            "Status", "ErrorMessage",
            "RetryCount", "MaxRetries",
            "CreatedAt"
        ) VALUES
        -- Completed jobs
        (gen_random_uuid(), v_tenant_id, v_rule_daily_report,
         NOW() - INTERVAL '18 hours', NOW() - INTERVAL '18 hours', NOW() - INTERVAL '18 hours' + INTERVAL '13 seconds',
         2, NULL, -- Completed
         0, 3,
         NOW() - INTERVAL '18 hours'),

        (gen_random_uuid(), v_tenant_id, v_rule_expiry_check,
         NOW() - INTERVAL '3 days', NOW() - INTERVAL '3 days', NOW() - INTERVAL '3 days' + INTERVAL '8 seconds',
         2, NULL, -- Completed
         0, 3,
         NOW() - INTERVAL '3 days'),

        -- Failed job (retried)
        (gen_random_uuid(), v_tenant_id, v_rule_daily_report,
         NOW() - INTERVAL '42 hours', NOW() - INTERVAL '42 hours', NOW() - INTERVAL '42 hours' + INTERVAL '20 seconds',
         3, 'Email delivery failed: SMTP connection timeout', -- Failed
         3, 3,
         NOW() - INTERVAL '42 hours'),

        -- Pending jobs (future)
        (gen_random_uuid(), v_tenant_id, v_rule_daily_report,
         NOW() + INTERVAL '6 hours', NULL, NULL,
         0, NULL, -- Pending
         0, 3,
         NOW()),

        (gen_random_uuid(), v_tenant_id, v_rule_expiry_check,
         NOW() + INTERVAL '4 days', NULL, NULL,
         0, NULL, -- Pending
         0, 3,
         NOW());

        RAISE NOTICE 'Successfully inserted 5 scheduled jobs';

    ELSE
        RAISE NOTICE 'Automation rules already exist, skipping seed';
    END IF;
END $$;

-- Useful queries:
-- SELECT * FROM "AutomationRules" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
-- SELECT * FROM "AutomationActionTemplates" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
-- SELECT ar.*, COUNT(re."Id") AS execution_count FROM "AutomationRules" ar LEFT JOIN "AutomationRuleExecutions" re ON ar."Id" = re."RuleId" WHERE ar."TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46' GROUP BY ar."Id";
-- SELECT * FROM "AutomationRuleExecutions" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46' ORDER BY "StartedAt" DESC LIMIT 20;
-- SELECT * FROM "AutomationScheduledJobs" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46' ORDER BY "ScheduledFor";
