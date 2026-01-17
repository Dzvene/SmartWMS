-- ============================================
-- SmartWMS - Seed Integrations Data
-- PostgreSQL
-- ============================================
-- TenantId: e9006ab8-257f-4021-b60a-cbba785bad46
-- ============================================
-- IntegrationType enum: 0=ERP, 1=Ecommerce, 2=Marketplace, 3=Shipping, 4=Accounting, 5=CRM, 6=WMS, 7=Custom, 8=API
-- IntegrationStatus enum: 0=Inactive, 1=Connecting, 2=Connected, 3=Error, 4=Suspended
-- IntegrationLogType enum: 0=Connection, 1=Authentication, 2=Sync, 3=Webhook, 4=Error, 5=Info
-- ============================================

DO $$
DECLARE
    v_tenant_id UUID := 'e9006ab8-257f-4021-b60a-cbba785bad46';
    v_integration_shopify UUID := 'aaaabbbb-0001-0001-0001-000000000001';
    v_integration_sap UUID := 'aaaabbbb-0001-0001-0002-000000000001';
    v_integration_dhl UUID := 'aaaabbbb-0001-0001-0003-000000000001';
    v_integration_xero UUID := 'aaaabbbb-0001-0001-0004-000000000001';
    v_integration_hubspot UUID := 'aaaabbbb-0001-0001-0005-000000000001';
    v_integration_amazon UUID := 'aaaabbbb-0001-0001-0006-000000000001';
    v_product_ids UUID[];
    v_customer_ids UUID[];
    v_order_ids UUID[];
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "Integrations" WHERE "TenantId" = v_tenant_id LIMIT 1) THEN
        -- ============================================
        -- Integrations
        -- ============================================
        INSERT INTO "Integrations" (
            "Id", "TenantId", "Name", "Code", "Type", "Description", "Provider",
            "BaseUrl", "ApiKey", "ApiSecret", "Username", "Password",
            "AccessToken", "TokenExpiresAt", "RefreshToken",
            "ConfigurationJson",
            "IsActive", "Status", "LastConnectedAt", "LastSyncAt", "LastError",
            "AutoSyncEnabled", "SyncIntervalMinutes", "NextSyncAt",
            "CreatedAt"
        ) VALUES
        -- Shopify Integration (Connected, active)
        (v_integration_shopify, v_tenant_id, 'Shopify Store', 'SHOPIFY', 1, 'Main e-commerce store integration', 'Shopify',
         'https://smartwms-demo.myshopify.com', 'shpat_xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx', NULL, NULL, NULL,
         'shpat_demo_access_token_12345', NOW() + INTERVAL '7 days', 'refresh_token_shopify_12345',
         '{"shop_name": "smartwms-demo", "api_version": "2024-01", "sync_products": true, "sync_orders": true, "sync_inventory": true}',
         true, 2, NOW() - INTERVAL '5 minutes', NOW() - INTERVAL '5 minutes', NULL,
         true, 15, NOW() + INTERVAL '15 minutes',
         NOW()),

        -- SAP ERP Integration (Connected)
        (v_integration_sap, v_tenant_id, 'SAP Business One', 'SAP-B1', 0, 'ERP system integration', 'SAP',
         'https://sap-demo.company.com:50000/b1s/v1', NULL, NULL, 'B1_INTEGRATION', 'encrypted_password_here',
         NULL, NULL, NULL,
         '{"company_db": "SBO_DEMO", "service_layer_version": "10.0", "sync_customers": true, "sync_products": true, "sync_orders": true}',
         true, 2, NOW() - INTERVAL '30 minutes', NOW() - INTERVAL '30 minutes', NULL,
         true, 60, NOW() + INTERVAL '30 minutes',
         NOW()),

        -- DHL Shipping Integration (Connected)
        (v_integration_dhl, v_tenant_id, 'DHL Express', 'DHL-EXPRESS', 3, 'DHL shipping integration', 'DHL',
         'https://express.api.dhl.com/mydhlapi', 'dhl_api_key_demo_12345', 'dhl_secret_demo_67890', NULL, NULL,
         NULL, NULL, NULL,
         '{"account_number": "123456789", "pickup_account": "987654321", "default_service": "EXPRESS_WORLDWIDE", "label_format": "PDF"}',
         true, 2, NOW() - INTERVAL '2 hours', NOW() - INTERVAL '1 hour', NULL,
         false, 0, NULL,
         NOW()),

        -- Xero Accounting (Error state)
        (v_integration_xero, v_tenant_id, 'Xero Accounting', 'XERO', 4, 'Accounting system integration', 'Xero',
         'https://api.xero.com/api.xro/2.0', NULL, NULL, NULL, NULL,
         'expired_xero_token', NOW() - INTERVAL '2 days', 'xero_refresh_token_expired',
         '{"tenant_id": "xero-tenant-uuid", "sync_invoices": true, "sync_payments": true, "default_account": "200"}',
         true, 3, NOW() - INTERVAL '2 days', NOW() - INTERVAL '2 days', 'Token refresh failed: Invalid refresh token',
         true, 60, NULL,
         NOW()),

        -- HubSpot CRM (Suspended)
        (v_integration_hubspot, v_tenant_id, 'HubSpot CRM', 'HUBSPOT', 5, 'CRM integration for customer sync', 'HubSpot',
         'https://api.hubapi.com', 'pat-na1-xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx', NULL, NULL, NULL,
         NULL, NULL, NULL,
         '{"sync_contacts": true, "sync_companies": true, "pipeline_id": "default"}',
         false, 4, NOW() - INTERVAL '30 days', NOW() - INTERVAL '30 days', 'Subscription expired',
         false, 0, NULL,
         NOW()),

        -- Amazon Marketplace (Inactive/New)
        (v_integration_amazon, v_tenant_id, 'Amazon Seller Central', 'AMAZON-SC', 2, 'Amazon marketplace integration', 'Amazon',
         'https://sellingpartnerapi-eu.amazon.com', NULL, NULL, NULL, NULL,
         NULL, NULL, NULL,
         '{"marketplace_id": "A1PA6795UKMFR9", "seller_id": "AXXXXXXXXXX"}',
         false, 0, NULL, NULL, NULL,
         false, 30, NULL,
         NOW());

        RAISE NOTICE 'Successfully inserted 6 integrations';

        -- ============================================
        -- Webhook Endpoints
        -- ============================================
        INSERT INTO "WebhookEndpoints" (
            "Id", "TenantId", "IntegrationId",
            "Name", "Url", "Secret", "Events",
            "IsActive", "RetryCount", "TimeoutSeconds",
            "LastTriggeredAt", "SuccessCount", "FailureCount",
            "CreatedAt"
        ) VALUES
        -- Shopify webhooks
        (gen_random_uuid(), v_tenant_id, v_integration_shopify,
         'Order Created', 'https://api.smartwms-demo.com/webhooks/shopify/orders/created', 'whsec_shopify_orders_secret_12345', ARRAY['orders/create'],
         true, 3, 30,
         NOW() - INTERVAL '2 hours', 156, 3,
         NOW()),

        (gen_random_uuid(), v_tenant_id, v_integration_shopify,
         'Order Updated', 'https://api.smartwms-demo.com/webhooks/shopify/orders/updated', 'whsec_shopify_orders_secret_12345', ARRAY['orders/updated', 'orders/cancelled'],
         true, 3, 30,
         NOW() - INTERVAL '4 hours', 89, 1,
         NOW()),

        (gen_random_uuid(), v_tenant_id, v_integration_shopify,
         'Product Updates', 'https://api.smartwms-demo.com/webhooks/shopify/products', 'whsec_shopify_products_secret_67890', ARRAY['products/create', 'products/update', 'products/delete'],
         true, 3, 30,
         NOW() - INTERVAL '1 day', 45, 0,
         NOW()),

        (gen_random_uuid(), v_tenant_id, v_integration_shopify,
         'Inventory Updates', 'https://api.smartwms-demo.com/webhooks/shopify/inventory', 'whsec_shopify_inventory_secret', ARRAY['inventory_levels/update'],
         true, 5, 60,
         NOW() - INTERVAL '30 minutes', 1250, 12,
         NOW()),

        -- DHL webhooks
        (gen_random_uuid(), v_tenant_id, v_integration_dhl,
         'Shipment Status', 'https://api.smartwms-demo.com/webhooks/dhl/tracking', 'whsec_dhl_tracking_secret', ARRAY['shipment.transit', 'shipment.delivered', 'shipment.exception'],
         true, 5, 45,
         NOW() - INTERVAL '3 hours', 320, 5,
         NOW()),

        -- SAP webhook (outbound only, usually)
        (gen_random_uuid(), v_tenant_id, v_integration_sap,
         'SAP Document Posted', 'https://api.smartwms-demo.com/webhooks/sap/documents', 'whsec_sap_doc_secret', ARRAY['invoice.posted', 'delivery.posted'],
         true, 3, 60,
         NOW() - INTERVAL '6 hours', 78, 2,
         NOW());

        RAISE NOTICE 'Successfully inserted 6 webhook endpoints';

        -- ============================================
        -- Integration Logs
        -- ============================================
        INSERT INTO "IntegrationLogs" (
            "Id", "TenantId", "IntegrationId",
            "LogType", "Direction", "EntityType", "EntityId", "ExternalId",
            "RequestData", "ResponseData", "HttpStatusCode",
            "Success", "ErrorMessage", "DurationMs",
            "CreatedAt"
        ) VALUES
        -- Shopify sync logs (recent)
        (gen_random_uuid(), v_tenant_id, v_integration_shopify,
         2, 'Out', 'StockLevel', NULL, NULL, -- Sync
         '{"location_id": "12345", "inventory_item_id": "67890", "available": 150}',
         '{"inventory_level": {"inventory_item_id": 67890, "available": 150}}',
         200, true, NULL, 234,
         NOW() - INTERVAL '5 minutes'),

        (gen_random_uuid(), v_tenant_id, v_integration_shopify,
         2, 'In', 'SalesOrder', NULL, 'shopify_order_5001234567890', -- Sync
         NULL,
         '{"order": {"id": 5001234567890, "order_number": "#1234", "total_price": "299.99"}}',
         200, true, NULL, 456,
         NOW() - INTERVAL '10 minutes'),

        (gen_random_uuid(), v_tenant_id, v_integration_shopify,
         3, 'In', 'SalesOrder', NULL, 'shopify_order_5001234567891', -- Webhook
         '{"id": 5001234567891, "order_number": "#1235", "financial_status": "paid"}',
         '{"success": true, "order_id": "SO-2024-0150"}',
         200, true, NULL, 1250,
         NOW() - INTERVAL '2 hours'),

        -- SAP sync logs
        (gen_random_uuid(), v_tenant_id, v_integration_sap,
         2, 'Out', 'SalesOrder', NULL, 'SAP_SO_12345', -- Sync
         '{"CardCode": "C10001", "DocDate": "2024-01-15", "DocumentLines": [...]}',
         '{"odata.metadata": "...", "DocEntry": 12345, "DocNum": 54321}',
         201, true, NULL, 890,
         NOW() - INTERVAL '30 minutes'),

        (gen_random_uuid(), v_tenant_id, v_integration_sap,
         0, NULL, NULL, NULL, NULL, -- Connection
         NULL,
         '{"SessionId": "xxxxx-xxxxx-xxxxx", "Version": "10.0"}',
         200, true, NULL, 345,
         NOW() - INTERVAL '35 minutes'),

        -- DHL shipping logs
        (gen_random_uuid(), v_tenant_id, v_integration_dhl,
         2, 'Out', 'Shipment', NULL, 'JD014600006753906201', -- Sync (create shipment)
         '{"plannedShippingDateAndTime": "2024-01-16T10:00:00", "pickup": {...}, "packages": [...]}',
         '{"shipmentTrackingNumber": "JD014600006753906201", "documents": [...]}',
         201, true, NULL, 2340,
         NOW() - INTERVAL '1 hour'),

        (gen_random_uuid(), v_tenant_id, v_integration_dhl,
         3, 'In', 'Shipment', NULL, 'JD014600006753906201', -- Webhook (tracking update)
         '{"shipmentTrackingNumber": "JD014600006753906201", "status": "transit", "location": "Leipzig Hub"}',
         '{"acknowledged": true}',
         200, true, NULL, 45,
         NOW() - INTERVAL '30 minutes'),

        -- Xero error logs
        (gen_random_uuid(), v_tenant_id, v_integration_xero,
         1, NULL, NULL, NULL, NULL, -- Authentication
         '{"grant_type": "refresh_token", "refresh_token": "xxxx"}',
         '{"error": "invalid_grant", "error_description": "The refresh token is invalid or has expired"}',
         401, false, 'Token refresh failed: Invalid refresh token', 234,
         NOW() - INTERVAL '2 days'),

        (gen_random_uuid(), v_tenant_id, v_integration_xero,
         4, NULL, NULL, NULL, NULL, -- Error
         NULL, NULL,
         NULL, false, 'Scheduled sync skipped due to authentication failure', 0,
         NOW() - INTERVAL '1 day'),

        -- Connection test log
        (gen_random_uuid(), v_tenant_id, v_integration_shopify,
         0, NULL, NULL, NULL, NULL, -- Connection
         NULL,
         '{"shop": {"id": 12345678, "name": "smartwms-demo", "email": "admin@example.com"}}',
         200, true, NULL, 567,
         NOW() - INTERVAL '1 hour');

        RAISE NOTICE 'Successfully inserted 10 integration logs';

        -- ============================================
        -- Integration Mappings
        -- ============================================
        -- Get some products and customers for mapping
        SELECT ARRAY_AGG("Id" ORDER BY "Sku")
        INTO v_product_ids
        FROM (SELECT "Id", "Sku" FROM "Products" WHERE "TenantId" = v_tenant_id AND "IsActive" = true ORDER BY "Sku" LIMIT 10) p;

        SELECT ARRAY_AGG("Id" ORDER BY "Code")
        INTO v_customer_ids
        FROM (SELECT "Id", "Code" FROM "Customers" WHERE "TenantId" = v_tenant_id AND "IsActive" = true ORDER BY "Code" LIMIT 5) c;

        SELECT ARRAY_AGG("Id" ORDER BY "OrderNumber")
        INTO v_order_ids
        FROM (SELECT "Id", "OrderNumber" FROM "SalesOrders" WHERE "TenantId" = v_tenant_id ORDER BY "OrderNumber" LIMIT 5) so;

        IF v_product_ids IS NOT NULL THEN
            INSERT INTO "IntegrationMappings" (
                "Id", "TenantId", "IntegrationId",
                "LocalEntityType", "LocalEntityId",
                "ExternalEntityType", "ExternalEntityId",
                "LastSyncAt", "LastSyncDirection", "SyncStatus",
                "CreatedAt"
            ) VALUES
            -- Shopify product mappings
            (gen_random_uuid(), v_tenant_id, v_integration_shopify,
             'Product', v_product_ids[1], 'product', 'shopify_product_7001234567890',
             NOW() - INTERVAL '1 day', 'Out', 'Synced', NOW()),

            (gen_random_uuid(), v_tenant_id, v_integration_shopify,
             'Product', v_product_ids[2], 'product', 'shopify_product_7001234567891',
             NOW() - INTERVAL '2 days', 'Out', 'Synced', NOW()),

            (gen_random_uuid(), v_tenant_id, v_integration_shopify,
             'Product', v_product_ids[3], 'product', 'shopify_product_7001234567892',
             NOW() - INTERVAL '3 days', 'Both', 'Synced', NOW()),

            -- SAP customer mappings
            (gen_random_uuid(), v_tenant_id, v_integration_sap,
             'Customer', v_customer_ids[1], 'BusinessPartner', 'C10001',
             NOW() - INTERVAL '1 hour', 'In', 'Synced', NOW()),

            (gen_random_uuid(), v_tenant_id, v_integration_sap,
             'Customer', v_customer_ids[2], 'BusinessPartner', 'C10002',
             NOW() - INTERVAL '2 hours', 'In', 'Synced', NOW()),

            -- SAP product mappings
            (gen_random_uuid(), v_tenant_id, v_integration_sap,
             'Product', v_product_ids[1], 'Item', 'SAP-ITEM-001',
             NOW() - INTERVAL '1 day', 'Both', 'Synced', NOW()),

            (gen_random_uuid(), v_tenant_id, v_integration_sap,
             'Product', v_product_ids[2], 'Item', 'SAP-ITEM-002',
             NOW() - INTERVAL '1 day', 'Both', 'Synced', NOW()),

            -- Order mappings
            (gen_random_uuid(), v_tenant_id, v_integration_shopify,
             'SalesOrder', v_order_ids[1], 'order', 'shopify_order_5001234567890',
             NOW() - INTERVAL '5 days', 'In', 'Synced', NOW()),

            (gen_random_uuid(), v_tenant_id, v_integration_shopify,
             'SalesOrder', v_order_ids[2], 'order', 'shopify_order_5001234567891',
             NOW() - INTERVAL '4 days', 'In', 'Synced', NOW()),

            (gen_random_uuid(), v_tenant_id, v_integration_sap,
             'SalesOrder', v_order_ids[1], 'SalesOrder', 'SAP_SO_12345',
             NOW() - INTERVAL '4 days', 'Out', 'Synced', NOW()),

            -- Xero mappings (stale due to integration error)
            (gen_random_uuid(), v_tenant_id, v_integration_xero,
             'Customer', v_customer_ids[1], 'Contact', 'xero_contact_uuid_001',
             NOW() - INTERVAL '3 days', 'Out', 'PendingSync', NOW()),

            (gen_random_uuid(), v_tenant_id, v_integration_xero,
             'SalesOrder', v_order_ids[1], 'Invoice', 'xero_invoice_uuid_001',
             NOW() - INTERVAL '3 days', 'Out', 'Error', NOW());

            RAISE NOTICE 'Successfully inserted 12 integration mappings';
        ELSE
            RAISE NOTICE 'No products found for mappings';
        END IF;

    ELSE
        RAISE NOTICE 'Integrations already exist, skipping seed';
    END IF;
END $$;

-- Useful queries:
-- SELECT * FROM "Integrations" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
-- SELECT i.*, COUNT(il."Id") AS log_count FROM "Integrations" i LEFT JOIN "IntegrationLogs" il ON i."Id" = il."IntegrationId" WHERE i."TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46' GROUP BY i."Id";
-- SELECT * FROM "WebhookEndpoints" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
-- SELECT * FROM "IntegrationLogs" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46' ORDER BY "CreatedAt" DESC LIMIT 20;
-- SELECT * FROM "IntegrationMappings" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
