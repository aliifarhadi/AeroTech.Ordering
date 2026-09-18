using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AeroTech.Ordering.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class S1DomainParityBackfill : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT coverage.[SegmentId]
    FROM [Order].[AirTransportServiceDetails] detail
    INNER JOIN [Order].[OrderServiceCoverage] coverage ON coverage.[ServiceId] = detail.[ServiceId]
    GROUP BY coverage.[SegmentId]
    HAVING COUNT(DISTINCT detail.[FlightNumber]) > 1
        OR COUNT(DISTINCT detail.[FlightVersion]) > 1
        OR COUNT(DISTINCT detail.[MarketingCarrierRef]) > 1
        OR COUNT(DISTINCT detail.[OperatingCarrierRef]) > 1)
    THROW 51001, 'S1 parity backfill: air services covering one segment disagree on its flight facts.', 1;

IF EXISTS (
    SELECT 1
    FROM [Order].[AirTransportServiceDetails]
    WHERE [FlightVersion] IS NOT NULL AND TRY_CONVERT(int, [FlightVersion]) IS NULL)
    THROW 51002, 'S1 parity backfill: an accepted flight version is not an invariant integer.', 1;

UPDATE segment
SET segment.[FlightNumber] = facts.[FlightNumber],
    segment.[FlightVersion] = facts.[FlightVersion],
    segment.[MarketingCarrierRef] = facts.[MarketingCarrierRef],
    segment.[OperatingCarrierRef] = facts.[OperatingCarrierRef]
FROM [Order].[OrderSegments] segment
INNER JOIN (
    SELECT coverage.[SegmentId] AS SegmentId,
           MAX(detail.[FlightNumber]) AS FlightNumber,
           MAX(detail.[FlightVersion]) AS FlightVersion,
           MAX(detail.[MarketingCarrierRef]) AS MarketingCarrierRef,
           MAX(detail.[OperatingCarrierRef]) AS OperatingCarrierRef
    FROM [Order].[AirTransportServiceDetails] detail
    INNER JOIN [Order].[OrderServiceCoverage] coverage ON coverage.[ServiceId] = detail.[ServiceId]
    GROUP BY coverage.[SegmentId]) facts ON facts.[SegmentId] = segment.[Id]
WHERE segment.[FlightNumber] IS NULL
  AND segment.[FlightVersion] IS NULL
  AND segment.[MarketingCarrierRef] IS NULL
  AND segment.[OperatingCarrierRef] IS NULL;
");

            migrationBuilder.Sql(@"
WITH units AS (
    SELECT construction.[Id] AS ConstructionId,
           unit.[key] AS UnitOrdinal,
           JSON_VALUE(unit.[value], '$.sourceUnitRef') AS SourceUnitRef,
           JSON_VALUE(unit.[value], '$.type') AS UnitType,
           JSON_QUERY(unit.[value], '$.coveredSourceBoundRefs') AS CoveredBounds,
           JSON_QUERY(unit.[value], '$.components') AS Components
    FROM [Order].[FareConstructions] construction
    CROSS APPLY OPENJSON(construction.[PricingUnitsJson]) unit
    WHERE construction.[PricingUnitsJson] IS NOT NULL
      AND ISJSON(construction.[PricingUnitsJson]) = 1
      AND NOT EXISTS (SELECT 1 FROM [Order].[FarePricingUnits] existing WHERE existing.[FareConstructionId] = construction.[Id]))
INSERT INTO [Order].[FarePricingUnits]
    ([Id], [FareConstructionId], [PricingGroupId], [Sequence], [SourceUnitRef], [SourceKindRaw], [Type], [CombinationMethod], [LastUpdateTime])
SELECT ROW_NUMBER() OVER (ORDER BY units.ConstructionId, units.UnitOrdinal),
       units.ConstructionId,
       NULL,
       CONVERT(int, units.UnitOrdinal) + 1,
       units.SourceUnitRef,
       NULL,
       CASE units.UnitType
           WHEN 'Unspecified' THEN 1
           WHEN 'OneWay' THEN 2
           WHEN 'RoundTrip' THEN 3
           WHEN 'OpenJaw' THEN 4
           WHEN 'CircleTrip' THEN 5
           WHEN 'Other' THEN 6
           ELSE 1
       END,
       1,
       SYSDATETIMEOFFSET()
FROM units;
");

            migrationBuilder.Sql(@"
WITH bounds AS (
    SELECT unit.[Id] AS PricingUnitId,
           bound.[value] AS SourceBoundRef,
           bound.[key] AS BoundOrdinal
    FROM [Order].[FareConstructions] construction
    CROSS APPLY OPENJSON(construction.[PricingUnitsJson]) json
    INNER JOIN [Order].[FarePricingUnits] unit
        ON unit.[FareConstructionId] = construction.[Id]
       AND unit.[SourceUnitRef] = JSON_VALUE(json.[value], '$.sourceUnitRef')
    CROSS APPLY OPENJSON(JSON_QUERY(json.[value], '$.coveredSourceBoundRefs')) bound
    WHERE construction.[PricingUnitsJson] IS NOT NULL
      AND ISJSON(construction.[PricingUnitsJson]) = 1
      AND NOT EXISTS (SELECT 1 FROM [Order].[FarePricingUnitCoveredBounds] existing WHERE existing.[PricingUnitId] = unit.[Id]))
INSERT INTO [Order].[FarePricingUnitCoveredBounds] ([Id], [PricingUnitId], [SourceBoundRef], [LastUpdateTime])
SELECT ROW_NUMBER() OVER (ORDER BY bounds.PricingUnitId, bounds.BoundOrdinal),
       bounds.PricingUnitId,
       bounds.SourceBoundRef,
       SYSDATETIMEOFFSET()
FROM bounds;
");

            migrationBuilder.Sql(@"
WITH components AS (
    SELECT unit.[Id] AS PricingUnitId,
           component.[key] AS ComponentOrdinal,
           JSON_VALUE(component.[value], '$.sourceFareRef') AS SourceFareRef,
           JSON_VALUE(component.[value], '$.fareBasis') AS FareBasis,
           JSON_VALUE(component.[value], '$.fareFamily') AS FareFamily,
           JSON_VALUE(component.[value], '$.fareType') AS FareType,
           JSON_VALUE(component.[value], '$.cabinRef') AS CabinRef,
           JSON_VALUE(component.[value], '$.rbdRef') AS RbdRef,
           JSON_VALUE(component.[value], '$.bookingClass') AS BookingClass
    FROM [Order].[FareConstructions] construction
    CROSS APPLY OPENJSON(construction.[PricingUnitsJson]) json
    INNER JOIN [Order].[FarePricingUnits] unit
        ON unit.[FareConstructionId] = construction.[Id]
       AND unit.[SourceUnitRef] = JSON_VALUE(json.[value], '$.sourceUnitRef')
    CROSS APPLY OPENJSON(JSON_QUERY(json.[value], '$.components')) component
    WHERE construction.[PricingUnitsJson] IS NOT NULL
      AND ISJSON(construction.[PricingUnitsJson]) = 1
      AND NOT EXISTS (SELECT 1 FROM [Order].[FareComponents] existing WHERE existing.[PricingUnitId] = unit.[Id]))
INSERT INTO [Order].[FareComponents]
    ([Id], [PricingUnitId], [Sequence], [SourceFareRef], [FareBasis], [FareFamily], [FareType], [CabinRef], [RbdRef],
     [BookingClass], [TicketingRestrictionMinutes], [FareOwnerRef], [TariffRef], [RuleRef], [RoutingRef], [LastUpdateTime])
SELECT ROW_NUMBER() OVER (ORDER BY components.PricingUnitId, components.ComponentOrdinal),
       components.PricingUnitId,
       CONVERT(int, components.ComponentOrdinal) + 1,
       components.SourceFareRef,
       components.FareBasis,
       components.FareFamily,
       components.FareType,
       components.CabinRef,
       components.RbdRef,
       components.BookingClass,
       NULL, NULL, NULL, NULL, NULL,
       SYSDATETIMEOFFSET()
FROM components;
");

            migrationBuilder.Sql(@"
WITH bindings AS (
    SELECT construction.[Id] AS ConstructionId,
           item.[Id] AS OrderItemId
    FROM [Order].[FareConstructions] construction
    INNER JOIN [Order].[OrderItems] item ON item.[OrderId] = construction.[OrderIdAtCreation]
    WHERE NOT EXISTS (SELECT 1 FROM [Order].[FareConstructionItems] existing WHERE existing.[FareConstructionId] = construction.[Id])
      AND (SELECT COUNT(1) FROM [Order].[OrderItems] sibling WHERE sibling.[OrderId] = construction.[OrderIdAtCreation]) = 1)
INSERT INTO [Order].[FareConstructionItems] ([Id], [FareConstructionId], [OrderItemId], [LastUpdateTime])
SELECT ROW_NUMBER() OVER (ORDER BY bindings.ConstructionId), bindings.ConstructionId, bindings.OrderItemId, SYSDATETIMEOFFSET()
FROM bindings;
");

            migrationBuilder.Sql(@"
WITH links AS (
    SELECT link.[Id] AS LinkId,
           beneficiary.[TravelerId] AS TravelerId,
           ROW_NUMBER() OVER (ORDER BY link.[Id], beneficiary.[TravelerId]) AS Ordinal
    FROM [Order].[OrderItemServiceLinks] link
    INNER JOIN [Order].[OrderServiceBeneficiaries] beneficiary ON beneficiary.[ServiceId] = link.[OrderServiceId]
    WHERE NOT EXISTS (SELECT 1 FROM [Order].[OrderItemServiceLinkTravelers] existing WHERE existing.[LinkId] = link.[Id]))
INSERT INTO [Order].[OrderItemServiceLinkTravelers] ([Id], [LinkId], [TravelerId], [LastUpdateTime])
SELECT links.Ordinal, links.LinkId, links.TravelerId, SYSDATETIMEOFFSET() FROM links;

WITH links AS (
    SELECT link.[Id] AS LinkId,
           coverage.[SegmentId] AS SegmentId,
           ROW_NUMBER() OVER (ORDER BY link.[Id], coverage.[SegmentId]) AS Ordinal
    FROM [Order].[OrderItemServiceLinks] link
    INNER JOIN [Order].[OrderServiceCoverage] coverage ON coverage.[ServiceId] = link.[OrderServiceId]
    WHERE NOT EXISTS (SELECT 1 FROM [Order].[OrderItemServiceLinkSegments] existing WHERE existing.[LinkId] = link.[Id]))
INSERT INTO [Order].[OrderItemServiceLinkSegments] ([Id], [LinkId], [SegmentId], [LastUpdateTime])
SELECT links.Ordinal, links.LinkId, links.SegmentId, SYSDATETIMEOFFSET() FROM links;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM [Order].[OrderItemServiceLinkSegments];
DELETE FROM [Order].[OrderItemServiceLinkTravelers];
DELETE FROM [Order].[FareConstructionItems];
DELETE FROM [Order].[FareComponents];
DELETE FROM [Order].[FarePricingUnitCoveredBounds];
DELETE FROM [Order].[FarePricingUnits];
UPDATE [Order].[OrderSegments]
SET [FlightNumber] = NULL, [FlightVersion] = NULL, [MarketingCarrierRef] = NULL, [OperatingCarrierRef] = NULL;
");
        }
    }
}
