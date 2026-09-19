# Benchmark Sources

Stage: 06-S1-create-order-conformance · retrieval date **2026-09-19**

Every vendor- or standards-specific claim used in `DOMAIN-BENCHMARK-MATRIX.md` traces to a row here. Where public material does not prove a detail, the row says `NOT PUBLICLY PROVEN` and the matrix must not assert it.

## Retrieval method and its limits

Pages were retrieved with the agent's web fetch (HTML → markdown → extraction). Two limits are recorded honestly:

- **Sabre pages return HTTP 403 to this agent.** `sabre.com/airline-mosaic/service-suite/order-management/order-management/` and `sabre.com/products/sabremosaic/order-processing/` were both refused; `investors.sabre.com` timed out. Sabre claims below therefore rest on search-result extracts of official Sabre pages, not on the retrieved page body. They are capped at **MEDIUM** confidence and no Sabre entity field is asserted anywhere.
- **IATA's normative data model is not on the open pages.** The Implementation Guides, AIDM and ONE Order schemas sit behind the developer portal. Field-level definitions of `OrderItem`, `Service`, `Price` are therefore `NOT PUBLICLY PROVEN` and are used only as concept names, never as field lists.

## IATA

| # | Source | URL | Claim supported | Confidence |
|---|---|---|---|---|
| I1 | "Fulfilment with Orders (ONE Order)" | `https://www.iata.org/en/programs/airline-distribution/retailing/one-order/` | ONE Order is "an XML-based standard that combines these multiple records into a single retail and customer focused Order" and aims to phase out "the current booking (PNRs) and ticketing records (e-tickets and electronic miscellaneous documents, or EMDs)". It "creates a single integrated customer record to streamline fulfilment, delivery, and accounting processes across the lifecycle of the Order." Names Order, Service, Customer, Delivery, Revenue Accounting. | HIGH |
| I2 | "Airline Retailing" (100% Offers and Orders) | `https://www.iata.org/en/programs/airline-distribution/retailing/` | The framework is built on **Offer** and **Order** as paired building blocks — sections titled "Retailing with Offers" and "Delivering with Orders" — plus Dynamic Offers, Settlement with Orders, the Retailing Consortium and a Reference Business Architecture. No fixed completion deadline; "timelines will vary depending on airline size, business models, and regional factors". | HIGH |
| I3 | "Settlement with Orders (SwO)" | `https://www.iata.org/en/programs/airline-distribution/retailing/settlement-orders-swo/` | SwO is "a framework for the settlement of orders between partners… a lean XML data exchange standard following a process agreed by the industry", applying to "settlement between Airlines and Sellers (Agents, OTAs, TMCs, etc.), using the current BSP agency program framework". | HIGH |
| I4 | Same page (I3) | as above | **How commission / agency remuneration relates to the customer-payable amount is not stated on the public page.** | `NOT PUBLICLY PROVEN` |
| I5 | IATA ONE Order material via site search (developer portal, fact sheet, AIR Tech Zone) | `https://developer.iata.org/en/one-order/`, `https://www.iata.org/en/iata-repository/pressroom/fact-sheets/fact-sheet-one-order/`, `https://airtechzone.iata.org/industry-programs/oo/` | "An Order may support non-homogeneity, i.e. each passenger in an Order may hold different sets of products and services." "A proposal to sell a specific set of products or Services under specific conditions, for a certain price forms the basis of order composition." "Order supports the sale of a flexible range of Airline products and Services that are not necessarily Journey based." | MEDIUM (search-result extract of official IATA pages; wording verified against two independent IATA URLs) |
| I6 | AIR Tech Zone "About ONE Order" | `https://airtechzone.iata.org/industry-programs/oo/` | Describes ONE Order as "a single Customer Order record, capturing all data elements obtained and required for order fulfillment of air travel" and states architectural principles only. **Entity-level definitions of Order / OrderItem / Service / Price are not on the public page.** | `NOT PUBLICLY PROVEN` for field detail; HIGH for the single-record principle |

## Amadeus

| # | Source | URL | Claim supported | Confidence |
|---|---|---|---|---|
| A1 | "Amadeus Nevio: Modern Airline Retailing" | `https://amadeus.com/en/airlines/products/nevio` | Nevio names **Offer Management**, **Order Management**, **Payment Management**, **Delivery Management** and **Traveler Experience Management** as its own capability names. Order Management is described as "processing and servicing of orders" and must "Ensure accurate fulfillment, delivery and accounting while staying fully in line with the One Order industry standard"; the Order record is referred to as a "single source of truth, driving simplification". | HIGH for the capability names and quoted wording |
| A2 | Same page (A1) | as above | **Nevio's public page does not name OrderItem, Service or traveller as discrete domain objects, and states no entity fields.** | `NOT PUBLICLY PROVEN` |

Amadeus Altéa is treated as part of the Amadeus product family, not as an independent standards authority. No Altéa-specific entity claim is made in this audit.

## Sabre

| # | Source | URL | Claim supported | Confidence |
|---|---|---|---|---|
| S1 | "Sabre introduces SabreMosaic™, its revolutionary Offer and Order retailing platform for airlines" (press release, 22 May 2024) | `https://www.sabre.com/insights/releases/sabre-introduces-sabremosaic-its-revolutionary-offer-and-order-retailing-platform-for-airlines/` | SabreMosaic "encompasses 10 new product suites – from offers and orders to settlement and delivery". | MEDIUM (search extract; page body refused with HTTP 403) |
| S2 | "Offer and order management: Sabre's airline strategy" | `https://www.sabre.com/resources/viewpoints/offer-and-order-from-strategy-to-scale/` | Sabre names the target model **offer, order, settle and deliver (OOSD)**: "The move to an offer, order, settle and deliver (OOSD) model will provide the experiences travelers demand". | MEDIUM (search extract; page body refused) |
| S3 | "SabreMosaic Order Processing" / "Order Management" | `https://www.sabre.com/products/sabremosaic/order-processing/`, `https://www.sabre.com/airline-mosaic/service-suite/order-management/order-management/` | "Sabre's advanced Order Management capabilities support the management and fulfillment of offers, including the acceptance and settlement of traditional and non-traditional payment types." Sabre Mosaic "is built for hybrid operations, allowing airlines to run modern offer-order capabilities alongside legacy systems at global scale". | MEDIUM (search extract; both pages returned HTTP 403 to direct fetch) |
| S4 | — | — | **No Sabre entity field, schema or attribute is asserted anywhere in this audit.** | `NOT PUBLICLY PROVEN` |

## Navitaire

| # | Source | URL | Claim supported | Confidence |
|---|---|---|---|---|
| N1 | "New Skies®" | `https://www.navitaire.com/new-skies-reservation-system` | New Skies is described as a "digital-first reservation, retailing and e-commerce system" built on "Single Order concepts from the beginning", and states "IATA has granted Navitaire Airline Retailing Maturity (ARM) status as a System Provider for our ONE Order-based solution". Ancillary merchandising wording: "Offer ancillaries that customers want, bundled or unbundled, at any passenger touchpoint" and "complementary products à la carte, in-path or post-sale". Settlement/accounting named as **SkyLedger Revenue Accounting**, **PRA System**, **Navitaire Interline AsSISt**. | HIGH for the quoted wording |
| N2 | Same page (N1) | as above | **New Skies' public page does not name seat, bag or a generic "Service" entity as distinct domain objects.** | `NOT PUBLICLY PROVEN` |
| N3 | Navitaire NDC | `https://navitaire.com/NDC` | Not separately retrieved in this run; no claim is made from it. | `NOT RETRIEVED` |

## How these sources are used

1. As a **semantic** benchmark: do industry-recognised concepts exist in the AeroTech model at all, under any name.
2. Never as a schema to copy. No vendor class name, field name or table is imported.
3. Never to override a recorded AeroTech owner decision. Where a benchmark exposes a concept the owner has not decided on, the matrix raises `BLOCKED_OWNER_CONTRACT` or an `OD-CLOSE-xx`, it does not change semantics.
4. Marketing pages are not used to manufacture a domain concept. Every `EXTERNAL_BENCHMARK_ONLY` row says plainly that it is benchmark context with no S1 obligation.
