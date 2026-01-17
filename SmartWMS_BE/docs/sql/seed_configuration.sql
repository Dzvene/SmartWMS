-- ============================================
-- SmartWMS - Seed Configuration Data
-- PostgreSQL
-- ============================================
-- TenantId: e9006ab8-257f-4021-b60a-cbba785bad46
-- ============================================
-- ReasonCodeType enum: 0=StockAdjustment, 1=Return, 2=Damage, 3=Scrap, 4=Transfer, 5=CycleCount, 6=OrderCancellation, 7=Receiving, 8=Shipping, 9=Other
-- BarcodePrefixType enum: 0=Product, 1=Location, 2=Bin, 3=Pallet, 4=Container, 5=User, 6=Equipment, 7=Document, 8=Other
-- ResetFrequency enum: 0=Never, 1=Daily, 2=Weekly, 3=Monthly, 4=Yearly
-- SettingValueType enum: 0=String, 1=Integer, 2=Decimal, 3=Boolean, 4=DateTime, 5=Json
-- ============================================

DO $$
DECLARE
    v_tenant_id UUID := 'e9006ab8-257f-4021-b60a-cbba785bad46';
BEGIN
    -- ==========================================
    -- REASON CODES
    -- ==========================================
    IF NOT EXISTS (SELECT 1 FROM "ReasonCodes" WHERE "TenantId" = v_tenant_id LIMIT 1) THEN

        INSERT INTO "ReasonCodes" (
            "Id", "TenantId", "Code", "Name", "Description",
            "ReasonType", "RequiresNotes", "RequiresApproval", "AffectsInventory",
            "IsActive", "SortOrder", "CreatedAt"
        ) VALUES
        -- Stock Adjustment Reasons
        (gen_random_uuid(), v_tenant_id, 'ADJ-COUNT', 'Cycle Count Adjustment', 'Variance found during cycle count', 0, false, false, true, true, 10, NOW()),
        (gen_random_uuid(), v_tenant_id, 'ADJ-DAMAGED', 'Damaged Goods', 'Items damaged in warehouse', 0, true, true, true, true, 20, NOW()),
        (gen_random_uuid(), v_tenant_id, 'ADJ-FOUND', 'Found Items', 'Items found during operations', 0, true, false, true, true, 30, NOW()),
        (gen_random_uuid(), v_tenant_id, 'ADJ-LOST', 'Lost Items', 'Items cannot be located', 0, true, true, true, true, 40, NOW()),
        (gen_random_uuid(), v_tenant_id, 'ADJ-EXPIRED', 'Expired Products', 'Products past expiry date', 0, false, false, true, true, 50, NOW()),
        (gen_random_uuid(), v_tenant_id, 'ADJ-QUALITY', 'Quality Hold', 'Items placed on quality hold', 0, true, true, true, true, 60, NOW()),
        (gen_random_uuid(), v_tenant_id, 'ADJ-OPENING', 'Opening Balance', 'Initial inventory setup', 0, false, false, true, true, 70, NOW()),

        -- Return Reasons
        (gen_random_uuid(), v_tenant_id, 'RET-DEFECT', 'Defective Product', 'Product has manufacturing defect', 1, true, false, true, true, 100, NOW()),
        (gen_random_uuid(), v_tenant_id, 'RET-WRONG', 'Wrong Item Shipped', 'Incorrect item sent to customer', 1, true, false, true, true, 110, NOW()),
        (gen_random_uuid(), v_tenant_id, 'RET-DAMAGE', 'Damaged in Transit', 'Item damaged during shipping', 1, true, false, true, true, 120, NOW()),
        (gen_random_uuid(), v_tenant_id, 'RET-NOTWANT', 'Customer Changed Mind', 'Customer no longer wants item', 1, false, false, true, true, 130, NOW()),
        (gen_random_uuid(), v_tenant_id, 'RET-DUPLICATE', 'Duplicate Order', 'Customer received duplicate', 1, false, false, true, true, 140, NOW()),

        -- Scrap Reasons
        (gen_random_uuid(), v_tenant_id, 'SCRAP-DAMAGED', 'Beyond Repair', 'Item damaged beyond repair', 3, true, true, true, true, 200, NOW()),
        (gen_random_uuid(), v_tenant_id, 'SCRAP-OBSOLETE', 'Obsolete Product', 'Product no longer sellable', 3, true, true, true, true, 210, NOW()),
        (gen_random_uuid(), v_tenant_id, 'SCRAP-RECALL', 'Product Recall', 'Manufacturer recall', 3, true, true, true, true, 220, NOW()),

        -- Transfer Reasons
        (gen_random_uuid(), v_tenant_id, 'TRF-REPLEN', 'Replenishment', 'Pick location replenishment', 4, false, false, true, true, 300, NOW()),
        (gen_random_uuid(), v_tenant_id, 'TRF-REORG', 'Warehouse Reorganization', 'Inventory reorganization', 4, false, false, true, true, 310, NOW()),
        (gen_random_uuid(), v_tenant_id, 'TRF-CONSOL', 'Consolidation', 'Consolidating inventory', 4, false, false, true, true, 320, NOW()),

        -- Order Cancellation Reasons
        (gen_random_uuid(), v_tenant_id, 'CAN-CUSTCANC', 'Customer Cancelled', 'Customer requested cancellation', 6, false, false, false, true, 400, NOW()),
        (gen_random_uuid(), v_tenant_id, 'CAN-NOSTOCK', 'Out of Stock', 'Items not available', 6, false, false, false, true, 410, NOW()),
        (gen_random_uuid(), v_tenant_id, 'CAN-PAYMENT', 'Payment Issue', 'Payment not received/declined', 6, true, false, false, true, 420, NOW()),
        (gen_random_uuid(), v_tenant_id, 'CAN-FRAUD', 'Fraud Suspected', 'Potential fraudulent order', 6, true, true, false, true, 430, NOW()),

        -- Receiving Reasons
        (gen_random_uuid(), v_tenant_id, 'RCV-DAMAGED', 'Received Damaged', 'Items arrived damaged', 7, true, false, true, true, 500, NOW()),
        (gen_random_uuid(), v_tenant_id, 'RCV-SHORT', 'Short Shipment', 'Received less than expected', 7, true, false, true, true, 510, NOW()),
        (gen_random_uuid(), v_tenant_id, 'RCV-OVER', 'Over Shipment', 'Received more than expected', 7, true, false, true, true, 520, NOW()),
        (gen_random_uuid(), v_tenant_id, 'RCV-WRONG', 'Wrong Items', 'Received incorrect items', 7, true, false, true, true, 530, NOW());

        RAISE NOTICE 'Successfully inserted 26 reason codes';
    END IF;

    -- ==========================================
    -- BARCODE PREFIXES
    -- ==========================================
    IF NOT EXISTS (SELECT 1 FROM "BarcodePrefixes" WHERE "TenantId" = v_tenant_id LIMIT 1) THEN

        INSERT INTO "BarcodePrefixes" (
            "Id", "TenantId", "Prefix", "Name", "Description",
            "PrefixType", "MinLength", "MaxLength", "Pattern",
            "IsActive", "CreatedAt"
        ) VALUES
        (gen_random_uuid(), v_tenant_id, 'P', 'Product SKU', 'Standard product barcode prefix', 0, 8, 20, '^P[A-Z0-9-]+$', true, NOW()),
        (gen_random_uuid(), v_tenant_id, 'L', 'Location Code', 'Location barcode prefix', 1, 5, 20, '^L[A-Z0-9-]+$', true, NOW()),
        (gen_random_uuid(), v_tenant_id, 'BIN', 'Bin Location', 'Bin location prefix', 2, 6, 15, '^BIN[0-9]+$', true, NOW()),
        (gen_random_uuid(), v_tenant_id, 'PLT', 'Pallet', 'Pallet identifier prefix', 3, 10, 20, '^PLT[0-9]{10,}$', true, NOW()),
        (gen_random_uuid(), v_tenant_id, 'CTN', 'Container', 'Container/carton prefix', 4, 10, 20, '^CTN[0-9]{10,}$', true, NOW()),
        (gen_random_uuid(), v_tenant_id, 'USR', 'User Badge', 'Employee badge barcode', 5, 8, 15, '^USR[0-9]+$', true, NOW()),
        (gen_random_uuid(), v_tenant_id, 'EQP', 'Equipment', 'Equipment ID prefix', 6, 8, 15, '^EQP[A-Z0-9-]+$', true, NOW()),
        (gen_random_uuid(), v_tenant_id, 'SO', 'Sales Order', 'Sales order document', 7, 10, 20, '^SO-[0-9]{4}-[0-9]+$', true, NOW()),
        (gen_random_uuid(), v_tenant_id, 'PO', 'Purchase Order', 'Purchase order document', 7, 10, 20, '^PO-[0-9]{4}-[0-9]+$', true, NOW()),
        (gen_random_uuid(), v_tenant_id, 'GR', 'Goods Receipt', 'Goods receipt document', 7, 10, 20, '^GR-[0-9]{4}-[0-9]+$', true, NOW()),
        (gen_random_uuid(), v_tenant_id, 'SHP', 'Shipment', 'Shipment document', 7, 10, 20, '^SHP-[0-9]{4}-[0-9]+$', true, NOW()),
        (gen_random_uuid(), v_tenant_id, 'RET', 'Return', 'Return order document', 7, 10, 20, '^RET-[0-9]{4}-[0-9]+$', true, NOW());

        RAISE NOTICE 'Successfully inserted 12 barcode prefixes';
    END IF;

    -- ==========================================
    -- NUMBER SEQUENCES
    -- ==========================================
    IF NOT EXISTS (SELECT 1 FROM "NumberSequences" WHERE "TenantId" = v_tenant_id LIMIT 1) THEN

        INSERT INTO "NumberSequences" (
            "Id", "TenantId", "SequenceType", "Prefix", "CurrentNumber",
            "IncrementBy", "MinDigits", "Suffix",
            "IncludeDate", "DateFormat", "ResetFrequency",
            "CreatedAt"
        ) VALUES
        (gen_random_uuid(), v_tenant_id, 'SalesOrder', 'SO', 100, 1, 4, NULL, true, 'yyyy', 4, NOW()),
        (gen_random_uuid(), v_tenant_id, 'PurchaseOrder', 'PO', 100, 1, 4, NULL, true, 'yyyy', 4, NOW()),
        (gen_random_uuid(), v_tenant_id, 'GoodsReceipt', 'GR', 100, 1, 4, NULL, true, 'yyyy', 4, NOW()),
        (gen_random_uuid(), v_tenant_id, 'Shipment', 'SHP', 1000, 1, 5, NULL, true, 'yyyy', 4, NOW()),
        (gen_random_uuid(), v_tenant_id, 'Return', 'RET', 100, 1, 4, NULL, true, 'yyyy', 4, NOW()),
        (gen_random_uuid(), v_tenant_id, 'StockMovement', 'MOV', 10000, 1, 5, NULL, true, 'yyyy', 4, NOW()),
        (gen_random_uuid(), v_tenant_id, 'StockAdjustment', 'ADJ', 100, 1, 4, NULL, true, 'yyyy', 4, NOW()),
        (gen_random_uuid(), v_tenant_id, 'StockTransfer', 'TRF', 100, 1, 4, NULL, true, 'yyyy', 4, NOW()),
        (gen_random_uuid(), v_tenant_id, 'CycleCount', 'CC', 100, 1, 4, NULL, true, 'yyyy', 4, NOW()),
        (gen_random_uuid(), v_tenant_id, 'PickTask', 'PICK', 10000, 1, 5, NULL, true, 'yyyy', 4, NOW()),
        (gen_random_uuid(), v_tenant_id, 'PackTask', 'PACK', 10000, 1, 5, NULL, true, 'yyyy', 4, NOW()),
        (gen_random_uuid(), v_tenant_id, 'PutawayTask', 'PUT', 10000, 1, 5, NULL, true, 'yyyy', 4, NOW()),
        (gen_random_uuid(), v_tenant_id, 'FulfillmentBatch', 'BATCH', 1000, 1, 4, NULL, true, 'yyyy', 4, NOW()),
        (gen_random_uuid(), v_tenant_id, 'DeliveryRoute', 'ROUTE', 100, 1, 4, NULL, true, 'yyyy', 4, NOW()),
        (gen_random_uuid(), v_tenant_id, 'Pallet', 'PLT', 100000, 1, 10, NULL, false, '', 0, NOW()),
        (gen_random_uuid(), v_tenant_id, 'Container', 'CTN', 100000, 1, 10, NULL, false, '', 0, NOW());

        RAISE NOTICE 'Successfully inserted 16 number sequences';
    END IF;

    -- ==========================================
    -- SYSTEM SETTINGS
    -- ==========================================
    IF NOT EXISTS (SELECT 1 FROM "SystemSettings" WHERE "TenantId" = v_tenant_id LIMIT 1) THEN

        INSERT INTO "SystemSettings" (
            "Id", "TenantId", "Category", "Key", "Value",
            "Description", "ValueType", "IsSystem", "CreatedAt"
        ) VALUES
        -- General Settings
        (gen_random_uuid(), v_tenant_id, 'General', 'CompanyName', 'SmartWMS Demo Company', 'Company display name', 0, false, NOW()),
        (gen_random_uuid(), v_tenant_id, 'General', 'DefaultTimezone', 'Europe/Stockholm', 'Default timezone for operations', 0, false, NOW()),
        (gen_random_uuid(), v_tenant_id, 'General', 'DefaultCurrency', 'SEK', 'Default currency code', 0, false, NOW()),
        (gen_random_uuid(), v_tenant_id, 'General', 'DateFormat', 'yyyy-MM-dd', 'Default date format', 0, false, NOW()),
        (gen_random_uuid(), v_tenant_id, 'General', 'TimeFormat', 'HH:mm', '24-hour time format', 0, false, NOW()),

        -- Inventory Settings
        (gen_random_uuid(), v_tenant_id, 'Inventory', 'AllowNegativeStock', 'false', 'Allow stock to go negative', 3, false, NOW()),
        (gen_random_uuid(), v_tenant_id, 'Inventory', 'DefaultReorderPoint', '20', 'Default reorder point for new products', 1, false, NOW()),
        (gen_random_uuid(), v_tenant_id, 'Inventory', 'EnableBatchTracking', 'true', 'Enable batch/lot tracking', 3, false, NOW()),
        (gen_random_uuid(), v_tenant_id, 'Inventory', 'EnableSerialTracking', 'true', 'Enable serial number tracking', 3, false, NOW()),
        (gen_random_uuid(), v_tenant_id, 'Inventory', 'EnableExpiryTracking', 'true', 'Enable expiry date tracking', 3, false, NOW()),
        (gen_random_uuid(), v_tenant_id, 'Inventory', 'ExpiryWarningDays', '30', 'Days before expiry to show warning', 1, false, NOW()),
        (gen_random_uuid(), v_tenant_id, 'Inventory', 'FIFOEnabled', 'true', 'Use FIFO for picking', 3, false, NOW()),

        -- Receiving Settings
        (gen_random_uuid(), v_tenant_id, 'Receiving', 'RequireQualityCheck', 'false', 'Require QC before putaway', 3, false, NOW()),
        (gen_random_uuid(), v_tenant_id, 'Receiving', 'AllowOverReceive', 'true', 'Allow receiving more than ordered', 3, false, NOW()),
        (gen_random_uuid(), v_tenant_id, 'Receiving', 'OverReceivePercent', '10', 'Max over-receive percentage', 1, false, NOW()),
        (gen_random_uuid(), v_tenant_id, 'Receiving', 'AutoGeneratePutaway', 'true', 'Auto-create putaway tasks', 3, false, NOW()),

        -- Picking Settings
        (gen_random_uuid(), v_tenant_id, 'Picking', 'DefaultPickStrategy', 'FIFO', 'Default pick strategy (FIFO, LIFO, FEFO)', 0, false, NOW()),
        (gen_random_uuid(), v_tenant_id, 'Picking', 'MaxPicksPerBatch', '50', 'Maximum picks per batch', 1, false, NOW()),
        (gen_random_uuid(), v_tenant_id, 'Picking', 'AllowPartialPicks', 'true', 'Allow partial quantity picks', 3, false, NOW()),
        (gen_random_uuid(), v_tenant_id, 'Picking', 'RequireLocationScan', 'true', 'Require location scan before pick', 3, false, NOW()),
        (gen_random_uuid(), v_tenant_id, 'Picking', 'RequireProductScan', 'true', 'Require product scan to confirm', 3, false, NOW()),

        -- Packing Settings
        (gen_random_uuid(), v_tenant_id, 'Packing', 'RequireWeightCapture', 'true', 'Require package weight', 3, false, NOW()),
        (gen_random_uuid(), v_tenant_id, 'Packing', 'RequireDimensionCapture', 'false', 'Require package dimensions', 3, false, NOW()),
        (gen_random_uuid(), v_tenant_id, 'Packing', 'AutoPrintLabel', 'true', 'Auto-print shipping label', 3, false, NOW()),
        (gen_random_uuid(), v_tenant_id, 'Packing', 'DefaultPackagingType', 'Box', 'Default package type', 0, false, NOW()),

        -- Shipping Settings
        (gen_random_uuid(), v_tenant_id, 'Shipping', 'DefaultCarrier', 'POSTNORD', 'Default carrier code', 0, false, NOW()),
        (gen_random_uuid(), v_tenant_id, 'Shipping', 'RequireTrackingNumber', 'true', 'Require tracking before ship', 3, false, NOW()),
        (gen_random_uuid(), v_tenant_id, 'Shipping', 'AutoCloseOnShip', 'true', 'Auto-close orders when shipped', 3, false, NOW()),

        -- Cycle Count Settings
        (gen_random_uuid(), v_tenant_id, 'CycleCount', 'AllowRecounts', 'true', 'Allow recounts on variance', 3, false, NOW()),
        (gen_random_uuid(), v_tenant_id, 'CycleCount', 'MaxRecounts', '3', 'Maximum recount attempts', 1, false, NOW()),
        (gen_random_uuid(), v_tenant_id, 'CycleCount', 'VarianceThreshold', '5', 'Auto-approve if variance % below', 2, false, NOW()),
        (gen_random_uuid(), v_tenant_id, 'CycleCount', 'RequireBlindCount', 'false', 'Hide expected qty from counter', 3, false, NOW()),

        -- Notification Settings
        (gen_random_uuid(), v_tenant_id, 'Notifications', 'LowStockAlert', 'true', 'Send low stock alerts', 3, false, NOW()),
        (gen_random_uuid(), v_tenant_id, 'Notifications', 'ExpiryAlert', 'true', 'Send expiry alerts', 3, false, NOW()),
        (gen_random_uuid(), v_tenant_id, 'Notifications', 'OrderDelayAlert', 'true', 'Send delayed order alerts', 3, false, NOW()),

        -- System Settings (locked)
        (gen_random_uuid(), v_tenant_id, 'System', 'Version', '1.0.0', 'System version', 0, true, NOW()),
        (gen_random_uuid(), v_tenant_id, 'System', 'DatabaseVersion', '7', 'Database migration version', 1, true, NOW());

        RAISE NOTICE 'Successfully inserted 37 system settings';
    END IF;

END $$;

-- Useful queries:
-- SELECT * FROM "ReasonCodes" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46' ORDER BY "SortOrder";
-- SELECT * FROM "BarcodePrefixes" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
-- SELECT * FROM "NumberSequences" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
-- SELECT * FROM "SystemSettings" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46' ORDER BY "Category", "Key";
