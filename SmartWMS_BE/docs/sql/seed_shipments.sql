-- ============================================
-- SmartWMS - Seed Shipments & Delivery Routes Data
-- PostgreSQL
-- ============================================
-- TenantId: e9006ab8-257f-4021-b60a-cbba785bad46
-- WarehouseId: ae5e6339-6212-4435-89a9-f5940815fe20
-- ============================================
-- ShipmentStatus enum: 0=Created, 1=Packed, 2=LabelPrinted, 3=PickedUp, 4=InTransit, 5=Delivered, 6=Cancelled
-- RouteStatus enum: 0=Draft, 1=Planned, 2=Released, 3=Loading, 4=InTransit, 5=Delivering, 6=Returning, 7=Complete, 8=Cancelled
-- StopStatus enum: 0=Pending, 1=EnRoute, 2=Arrived, 3=Delivering, 4=Complete, 5=PartialDelivery, 6=Failed, 7=Skipped, 8=Rescheduled
-- ============================================

DO $$
DECLARE
    v_tenant_id UUID := 'e9006ab8-257f-4021-b60a-cbba785bad46';
    v_warehouse_id UUID := 'ae5e6339-6212-4435-89a9-f5940815fe20';
    v_carrier_dhl UUID := '33333333-0001-0001-0001-000000000001';
    v_carrier_postnord UUID := '33333333-0004-0001-0001-000000000001';
    v_sales_order_ids UUID[];
    v_customer_ids UUID[];
    v_shipment_id UUID;
    v_route_id UUID;
    v_stop_id UUID;
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "Shipments" WHERE "TenantId" = v_tenant_id LIMIT 1) THEN
        -- Get sales orders
        SELECT ARRAY_AGG("Id" ORDER BY "OrderNumber")
        INTO v_sales_order_ids
        FROM (SELECT "Id", "OrderNumber" FROM "SalesOrders"
              WHERE "TenantId" = v_tenant_id ORDER BY "OrderNumber" LIMIT 8) so;

        -- Get customers for shipping addresses
        SELECT ARRAY_AGG("Id" ORDER BY "Code")
        INTO v_customer_ids
        FROM (SELECT "Id", "Code" FROM "Customers"
              WHERE "TenantId" = v_tenant_id AND "IsActive" = true ORDER BY "Code" LIMIT 8) c;

        IF v_sales_order_ids IS NULL THEN
            RAISE NOTICE 'No sales orders found';
            RETURN;
        END IF;

        -- Shipment 1: Delivered
        v_shipment_id := gen_random_uuid();
        INSERT INTO "Shipments" (
            "Id", "TenantId", "ShipmentNumber", "OrderId", "WarehouseId",
            "Status", "CarrierCode", "CarrierName", "ServiceLevel",
            "TrackingNumber", "TrackingUrl",
            "PackageCount", "TotalWeightKg", "WidthMm", "HeightMm", "DepthMm",
            "ShipToName", "ShipToAddressLine1", "ShipToCity", "ShipToRegion", "ShipToPostalCode", "ShipToCountryCode",
            "ShippedAt", "DeliveredAt", "ShippingCost", "CurrencyCode",
            "Notes", "CreatedAt"
        ) VALUES (
            v_shipment_id, v_tenant_id, 'SHP-2024-00001', v_sales_order_ids[1], v_warehouse_id,
            5, 'DHL', 'DHL Express', 'Express Worldwide', -- Delivered
            'JD014600006753906201', 'https://www.dhl.com/tracking?id=JD014600006753906201',
            2, 8.5, 400, 300, 200,
            'Acme Corporation', '123 Business Park', 'New York', 'NY', '10001', 'US',
            NOW() - INTERVAL '5 days', NOW() - INTERVAL '3 days', 125.50, 'SEK',
            'Delivered on time', NOW()
        );

        -- Shipment 2: In Transit
        INSERT INTO "Shipments" (
            "Id", "TenantId", "ShipmentNumber", "OrderId", "WarehouseId",
            "Status", "CarrierCode", "CarrierName", "ServiceLevel",
            "TrackingNumber", "TrackingUrl",
            "PackageCount", "TotalWeightKg",
            "ShipToName", "ShipToAddressLine1", "ShipToCity", "ShipToRegion", "ShipToPostalCode", "ShipToCountryCode",
            "ShippedAt", "ShippingCost", "CurrencyCode",
            "CreatedAt"
        ) VALUES (
            gen_random_uuid(), v_tenant_id, 'SHP-2024-00002', v_sales_order_ids[2], v_warehouse_id,
            4, 'POSTNORD', 'PostNord', 'MyPack Home', -- InTransit
            'SE1234567890', 'https://www.postnord.se/tracking?id=SE1234567890',
            1, 3.2,
            'TechStart AB', 'Drottninggatan 50', 'Stockholm', 'Stockholm', '111 21', 'SE',
            NOW() - INTERVAL '1 day', 65.00, 'SEK',
            NOW()
        );

        -- Shipment 3: Picked Up
        INSERT INTO "Shipments" (
            "Id", "TenantId", "ShipmentNumber", "OrderId", "WarehouseId",
            "Status", "CarrierCode", "CarrierName", "ServiceLevel",
            "TrackingNumber",
            "PackageCount", "TotalWeightKg",
            "ShipToName", "ShipToAddressLine1", "ShipToCity", "ShipToPostalCode", "ShipToCountryCode",
            "ShippedAt", "ShippingCost", "CurrencyCode",
            "CreatedAt"
        ) VALUES (
            gen_random_uuid(), v_tenant_id, 'SHP-2024-00003', v_sales_order_ids[3], v_warehouse_id,
            3, 'DHL', 'DHL Express', 'Economy Select', -- PickedUp
            'JD014600006753906203',
            1, 5.8,
            'Global Logistics GmbH', 'Industriestrasse 42', 'Berlin', '10115', 'DE',
            NOW() - INTERVAL '4 hours', 89.00, 'SEK',
            NOW()
        );

        -- Shipment 4: Label Printed
        INSERT INTO "Shipments" (
            "Id", "TenantId", "ShipmentNumber", "OrderId", "WarehouseId",
            "Status", "CarrierCode", "CarrierName", "ServiceLevel",
            "TrackingNumber",
            "PackageCount", "TotalWeightKg",
            "ShipToName", "ShipToAddressLine1", "ShipToCity", "ShipToPostalCode", "ShipToCountryCode",
            "ShippingCost", "CurrencyCode",
            "CreatedAt"
        ) VALUES (
            gen_random_uuid(), v_tenant_id, 'SHP-2024-00004', v_sales_order_ids[4], v_warehouse_id,
            2, 'FEDEX', 'FedEx', 'International Priority', -- LabelPrinted
            'FX123456789012',
            1, 2.5,
            'Nordic Supplies', 'Storgata 15', 'Oslo', '0154', 'NO',
            145.00, 'SEK',
            NOW()
        );

        -- Shipment 5: Created (packed, awaiting label)
        INSERT INTO "Shipments" (
            "Id", "TenantId", "ShipmentNumber", "OrderId", "WarehouseId",
            "Status", "CarrierCode", "CarrierName", "ServiceLevel",
            "PackageCount", "TotalWeightKg",
            "ShipToName", "ShipToAddressLine1", "ShipToCity", "ShipToPostalCode", "ShipToCountryCode",
            "CreatedAt"
        ) VALUES (
            gen_random_uuid(), v_tenant_id, 'SHP-2024-00005', v_sales_order_ids[5], v_warehouse_id,
            1, 'POSTNORD', 'PostNord', 'MyPack Collect', -- Packed
            2, 12.3,
            'Pacific Trading Co', '888 Market Street', 'San Francisco', '94102', 'US',
            NOW()
        );

        -- Shipment 6: Cancelled
        INSERT INTO "Shipments" (
            "Id", "TenantId", "ShipmentNumber", "OrderId", "WarehouseId",
            "Status", "CarrierCode", "CarrierName",
            "PackageCount", "TotalWeightKg",
            "ShipToName", "ShipToAddressLine1", "ShipToCity", "ShipToCountryCode",
            "Notes", "CreatedAt"
        ) VALUES (
            gen_random_uuid(), v_tenant_id, 'SHP-2024-00006', v_sales_order_ids[6], v_warehouse_id,
            6, 'UPS', 'UPS', -- Cancelled
            1, 4.0,
            'Cancelled Customer', 'Some Address', 'Some City', 'SE',
            'Order cancelled before shipment', NOW()
        );

        RAISE NOTICE 'Successfully inserted 6 shipments';
    END IF;

    -- Create Delivery Routes
    IF NOT EXISTS (SELECT 1 FROM "DeliveryRoutes" WHERE "TenantId" = v_tenant_id LIMIT 1) THEN
        -- Route 1: Complete route
        v_route_id := gen_random_uuid();
        INSERT INTO "DeliveryRoutes" (
            "Id", "TenantId", "RouteNumber", "RouteName", "WarehouseId",
            "CarrierId", "DriverName", "VehicleNumber", "VehicleType",
            "PlannedDate", "PlannedDepartureTime", "PlannedReturnTime",
            "ActualDepartureTime", "ActualReturnTime",
            "EstimatedStops", "EstimatedDistance", "EstimatedDuration",
            "Status", "MaxWeight", "MaxStops", "CurrentWeight", "CurrentVolume",
            "Notes", "CreatedAt"
        ) VALUES (
            v_route_id, v_tenant_id, 'ROUTE-2024-0001', 'Stockholm Metro AM', v_warehouse_id,
            v_carrier_postnord, 'Erik Svensson', 'ABC123', 'Van',
            NOW() - INTERVAL '2 days', NOW() - INTERVAL '2 days' + INTERVAL '8 hours', NOW() - INTERVAL '2 days' + INTERVAL '14 hours',
            NOW() - INTERVAL '2 days' + INTERVAL '8 hours 15 minutes', NOW() - INTERVAL '2 days' + INTERVAL '13 hours 45 minutes',
            5, 45.5, 300,
            7, 1000, 10, 250, 1.5, -- Complete
            'Morning delivery route - Stockholm metro area', NOW()
        );

        -- Add stops to route 1
        INSERT INTO "DeliveryStops" (
            "Id", "TenantId", "RouteId", "StopSequence",
            "CustomerName", "AddressLine1", "City", "PostalCode", "Country",
            "ContactPhone", "DeliveryInstructions",
            "TimeWindowStart", "TimeWindowEnd", "EstimatedServiceTime",
            "ArrivalTime", "DepartureTime",
            "Status", "SignedBy", "SignedAt", "CreatedAt"
        ) VALUES
        (gen_random_uuid(), v_tenant_id, v_route_id, 1, 'Customer A', 'Address 1', 'Stockholm', '111 21', 'SE',
         '+46701234567', 'Ring doorbell', NOW() - INTERVAL '2 days' + INTERVAL '9 hours', NOW() - INTERVAL '2 days' + INTERVAL '10 hours',
         15, NOW() - INTERVAL '2 days' + INTERVAL '9 hours 10 minutes', NOW() - INTERVAL '2 days' + INTERVAL '9 hours 25 minutes',
         4, 'Anna Lindgren', NOW() - INTERVAL '2 days' + INTERVAL '9 hours 20 minutes', NOW()),
        (gen_random_uuid(), v_tenant_id, v_route_id, 2, 'Customer B', 'Address 2', 'Stockholm', '112 45', 'SE',
         '+46707654321', NULL, NOW() - INTERVAL '2 days' + INTERVAL '10 hours', NOW() - INTERVAL '2 days' + INTERVAL '11 hours',
         10, NOW() - INTERVAL '2 days' + INTERVAL '10 hours 15 minutes', NOW() - INTERVAL '2 days' + INTERVAL '10 hours 28 minutes',
         4, 'Johan Berg', NOW() - INTERVAL '2 days' + INTERVAL '10 hours 25 minutes', NOW()),
        (gen_random_uuid(), v_tenant_id, v_route_id, 3, 'Customer C', 'Address 3', 'Solna', '171 23', 'SE',
         '+46709876543', 'Leave at door', NOW() - INTERVAL '2 days' + INTERVAL '11 hours', NOW() - INTERVAL '2 days' + INTERVAL '12 hours',
         10, NOW() - INTERVAL '2 days' + INTERVAL '11 hours 20 minutes', NOW() - INTERVAL '2 days' + INTERVAL '11 hours 35 minutes',
         4, 'Maria K', NOW() - INTERVAL '2 days' + INTERVAL '11 hours 32 minutes', NOW());

        -- Route 2: In Transit
        v_route_id := gen_random_uuid();
        INSERT INTO "DeliveryRoutes" (
            "Id", "TenantId", "RouteNumber", "RouteName", "WarehouseId",
            "CarrierId", "DriverName", "VehicleNumber", "VehicleType",
            "PlannedDate", "PlannedDepartureTime", "PlannedReturnTime",
            "ActualDepartureTime",
            "EstimatedStops", "EstimatedDistance", "EstimatedDuration",
            "Status", "MaxWeight", "CurrentWeight", "CurrentVolume",
            "Notes", "CreatedAt"
        ) VALUES (
            v_route_id, v_tenant_id, 'ROUTE-2024-0002', 'Uppsala Route', v_warehouse_id,
            v_carrier_postnord, 'Lars Andersson', 'DEF456', 'Van',
            NOW(), NOW() + INTERVAL '8 hours', NOW() + INTERVAL '16 hours',
            NOW() + INTERVAL '8 hours 5 minutes',
            4, 85.0, 420,
            4, 1000, 320, 2.0, -- InTransit
            'Uppsala area deliveries', NOW()
        );

        INSERT INTO "DeliveryStops" (
            "Id", "TenantId", "RouteId", "StopSequence",
            "CustomerName", "AddressLine1", "City", "PostalCode", "Country",
            "TimeWindowStart", "TimeWindowEnd",
            "Status", "CreatedAt"
        ) VALUES
        (gen_random_uuid(), v_tenant_id, v_route_id, 1, 'Uppsala Customer 1', 'Svartbäcksgatan 1', 'Uppsala', '753 20', 'SE',
         NOW() + INTERVAL '10 hours', NOW() + INTERVAL '11 hours', 1, NOW()), -- EnRoute
        (gen_random_uuid(), v_tenant_id, v_route_id, 2, 'Uppsala Customer 2', 'Kungsgatan 45', 'Uppsala', '753 21', 'SE',
         NOW() + INTERVAL '11 hours', NOW() + INTERVAL '12 hours', 0, NOW()), -- Pending
        (gen_random_uuid(), v_tenant_id, v_route_id, 3, 'Uppsala Customer 3', 'Dragarbrunnsgatan 78', 'Uppsala', '753 22', 'SE',
         NOW() + INTERVAL '13 hours', NOW() + INTERVAL '14 hours', 0, NOW()); -- Pending

        -- Route 3: Planned (not started)
        INSERT INTO "DeliveryRoutes" (
            "Id", "TenantId", "RouteNumber", "RouteName", "WarehouseId",
            "CarrierId", "DriverName", "VehicleNumber", "VehicleType",
            "PlannedDate", "PlannedDepartureTime", "PlannedReturnTime",
            "EstimatedStops", "EstimatedDistance", "EstimatedDuration",
            "Status", "MaxWeight", "CurrentWeight", "CurrentVolume",
            "Notes", "CreatedAt"
        ) VALUES (
            gen_random_uuid(), v_tenant_id, 'ROUTE-2024-0003', 'Tomorrow South Route', v_warehouse_id,
            v_carrier_postnord, 'Not Assigned', NULL, 'Van',
            NOW() + INTERVAL '1 day', NOW() + INTERVAL '1 day' + INTERVAL '8 hours', NOW() + INTERVAL '1 day' + INTERVAL '15 hours',
            6, 65.0, 360,
            1, 1000, 0, 0, -- Planned
            'South Stockholm deliveries', NOW()
        );

        -- Route 4: Draft
        INSERT INTO "DeliveryRoutes" (
            "Id", "TenantId", "RouteNumber", "RouteName", "WarehouseId",
            "PlannedDate",
            "EstimatedStops",
            "Status", "MaxWeight", "CurrentWeight", "CurrentVolume",
            "Notes", "CreatedAt"
        ) VALUES (
            gen_random_uuid(), v_tenant_id, 'ROUTE-2024-0004', 'Draft Route', v_warehouse_id,
            NOW() + INTERVAL '3 days',
            3,
            0, 1000, 0, 0, -- Draft
            'Planning in progress', NOW()
        );

        RAISE NOTICE 'Successfully inserted 4 delivery routes with stops';
    END IF;

    -- Create Delivery Tracking Events
    IF NOT EXISTS (SELECT 1 FROM "DeliveryTrackingEvents" WHERE "TenantId" = v_tenant_id LIMIT 1) THEN
        SELECT "Id" INTO v_route_id FROM "DeliveryRoutes"
        WHERE "TenantId" = v_tenant_id AND "RouteNumber" = 'ROUTE-2024-0001' LIMIT 1;

        IF v_route_id IS NOT NULL THEN
            INSERT INTO "DeliveryTrackingEvents" (
                "Id", "TenantId", "RouteId", "StopId", "ShipmentId",
                "EventType", "EventTime", "EventDescription",
                "Latitude", "Longitude", "LocationDescription",
                "PerformedBy", "CreatedAt"
            ) VALUES
            (gen_random_uuid(), v_tenant_id, v_route_id, NULL, NULL,
             1, NOW() - INTERVAL '2 days' + INTERVAL '7 hours 45 minutes', 'Route released for delivery', -- Released
             NULL, NULL, 'Warehouse', 'System', NOW()),
            (gen_random_uuid(), v_tenant_id, v_route_id, NULL, NULL,
             2, NOW() - INTERVAL '2 days' + INTERVAL '8 hours', 'Loading started', -- LoadingStarted
             59.3293, 18.0686, 'Stockholm Main Warehouse', 'Erik Svensson', NOW()),
            (gen_random_uuid(), v_tenant_id, v_route_id, NULL, NULL,
             3, NOW() - INTERVAL '2 days' + INTERVAL '8 hours 15 minutes', 'Loading complete', -- LoadingComplete
             59.3293, 18.0686, 'Stockholm Main Warehouse', 'Erik Svensson', NOW()),
            (gen_random_uuid(), v_tenant_id, v_route_id, NULL, NULL,
             4, NOW() - INTERVAL '2 days' + INTERVAL '8 hours 20 minutes', 'Departed warehouse', -- Departed
             59.3293, 18.0686, 'Stockholm Main Warehouse', 'Erik Svensson', NOW()),
            (gen_random_uuid(), v_tenant_id, v_route_id, NULL, NULL,
             5, NOW() - INTERVAL '2 days' + INTERVAL '9 hours', 'Location update', -- LocationUpdate
             59.3310, 18.0700, 'En route to first stop', NULL, NOW()),
            (gen_random_uuid(), v_tenant_id, v_route_id, NULL, NULL,
             12, NOW() - INTERVAL '2 days' + INTERVAL '13 hours 45 minutes', 'Returned to warehouse', -- ReturnedToWarehouse
             59.3293, 18.0686, 'Stockholm Main Warehouse', 'Erik Svensson', NOW());
        END IF;

        RAISE NOTICE 'Successfully inserted delivery tracking events';
    END IF;
END $$;

-- ShipmentStatus enum reference:
-- 0 = Created, 1 = Packed, 2 = LabelPrinted, 3 = PickedUp, 4 = InTransit, 5 = Delivered, 6 = Cancelled

-- RouteStatus enum reference:
-- 0 = Draft, 1 = Planned, 2 = Released, 3 = Loading, 4 = InTransit, 5 = Delivering, 6 = Returning, 7 = Complete, 8 = Cancelled

-- Useful queries:
-- SELECT * FROM "Shipments" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
-- SELECT * FROM "DeliveryRoutes" WHERE "TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
-- SELECT dr.*, ds.* FROM "DeliveryRoutes" dr LEFT JOIN "DeliveryStops" ds ON dr."Id" = ds."RouteId" WHERE dr."TenantId" = 'e9006ab8-257f-4021-b60a-cbba785bad46';
