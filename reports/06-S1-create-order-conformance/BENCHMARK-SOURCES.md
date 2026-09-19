# Benchmark Sources

Stage: 06-S1-create-order-conformance · retrieval date **2026-09-19** · revision 2 (AIDM added)

Every vendor- or standards-specific claim used in `DOMAIN-BENCHMARK-MATRIX.md` traces to a row here. Where public material does not prove a detail, the row says `NOT PUBLICLY PROVEN` and the matrix must not assert it.

## Retrieval method and its limits

Pages were retrieved with the agent's web fetch (HTML → markdown → extraction). Two limits are recorded honestly:

- **Sabre pages return HTTP 403 to this agent.** `sabre.com/airline-mosaic/service-suite/order-management/order-management/` and `sabre.com/products/sabremosaic/order-processing/` were both refused; `investors.sabre.com` timed out. Sabre claims below therefore rest on search-result extracts of official Sabre pages, not on the retrieved page body. They are capped at **MEDIUM** confidence and no Sabre entity field is asserted anywhere.
- **Correction from revision 1.** Revision 1 claimed "the Implementation Guides, AIDM and ONE Order schemas sit behind the developer portal" and marked every IATA entity definition `NOT PUBLICLY PROVEN`. That was true of the Implementation Guides and **wrong about AIDM**, which publishes entity pages openly at `airtechzone.iata.org/aidm_model/`. Five AIDM entity pages were retrieved directly in revision 2 and are cited at HIGH confidence below. The `NOT PUBLICLY PROVEN` markers that rested on that mistake are withdrawn.

## IATA

| # | Source | URL | Claim supported | Confidence |
|---|---|---|---|---|
| I1 | "Fulfilment with Orders (ONE Order)" | `https://www.iata.org/en/programs/airline-distribution/retailing/one-order/` | ONE Order is "an XML-based standard that combines these multiple records into a single retail and customer focused Order" and aims to phase out "the current booking (PNRs) and ticketing records (e-tickets and electronic miscellaneous documents, or EMDs)". It "creates a single integrated customer record to streamline fulfilment, delivery, and accounting processes across the lifecycle of the Order." Names Order, Service, Customer, Delivery, Revenue Accounting. | HIGH |
| I2 | "Airline Retailing" (100% Offers and Orders) | `https://www.iata.org/en/programs/airline-distribution/retailing/` | The framework is built on **Offer** and **Order** as paired building blocks — sections titled "Retailing with Offers" and "Delivering with Orders" — plus Dynamic Offers, Settlement with Orders, the Retailing Consortium and a Reference Business Architecture. No fixed completion deadline; "timelines will vary depending on airline size, business models, and regional factors". | HIGH |
| I3 | "Settlement with Orders (SwO)" | `https://www.iata.org/en/programs/airline-distribution/retailing/settlement-orders-swo/` | SwO is "a framework for the settlement of orders between partners… a lean XML data exchange standard following a process agreed by the industry", applying to "settlement between Airlines and Sellers (Agents, OTAs, TMCs, etc.), using the current BSP agency program framework". | HIGH |
| I4 | Same page (I3) | as above | **How commission / agency remuneration relates to the customer-payable amount is not stated on the public page.** | `NOT PUBLICLY PROVEN` |
| I5 | IATA ONE Order material via site search (developer portal, fact sheet, AIR Tech Zone) | `https://developer.iata.org/en/one-order/`, `https://www.iata.org/en/iata-repository/pressroom/fact-sheets/fact-sheet-one-order/`, `https://airtechzone.iata.org/industry-programs/oo/` | "An Order may support non-homogeneity, i.e. each passenger in an Order may hold different sets of products and services." "A proposal to sell a specific set of products or Services under specific conditions, for a certain price forms the basis of order composition." "Order supports the sale of a flexible range of Airline products and Services that are not necessarily Journey based." | MEDIUM (search-result extract of official IATA pages; wording verified against two independent IATA URLs) |
| I6 | AIR Tech Zone "About ONE Order" | `https://airtechzone.iata.org/industry-programs/oo/` | Describes ONE Order as "a single Customer Order record, capturing all data elements obtained and required for order fulfillment of air travel" and states architectural principles only. Entity definitions live in AIDM, not on this page. | HIGH for the single-record principle |
| I7 | "Business Reference Architecture" | `https://www.iata.org/reference-architecture` | The architecture "outlines the impact of the transition to Offers and Orders on various financial management capabilities, including **Customer Order Accounting, Partners/Suppliers Order Accounting**, General Accounting & Revenue Recognition, Corporate Finance, Treasury & Risks, Enterprise Performance Management, and Tax Management." It is "a framework, agnostic to specific technologies". | HIGH — corroborates that customer-payable accounting and partner/supplier settlement are **separate** capabilities |

## IATA AIDM (public entity model) — added in revision 2

| # | Source | URL | Claim supported | Confidence |
|---|---|---|---|---|
| M1 | AIDM 25.2 **Order Item** | `https://airtechzone.iata.org/aidm_model/25.2/EARoot/EA6/EA1/EA2/EA7/EA10550.htm` | Definition: "An individually priced item within an Order, made up of one or more Services. May or may not be a selected Offer Item (e.g. non-chargeable Services available on request such as a wheelchair)." Attributes include Order Item Identifier, Status Code, Type Code, **Grand Total Amount**, and four separate limits (Payment, Price Guarantee, Ticketing, Naming Time Limit). Associations include Service (strong), Price (strong), **Commission (may be present)**, Change Restrictions, Cancel Restrictions, Penalty, Payment Information, Offer Item (weak). | HIGH |
| M2 | AIDM 25.2 **Service** | `https://airtechzone.iata.org/aidm_model/25.2/EARoot/EA6/EA2/EA2/EA3/EA11503.htm` | Definition: "An instance of a specific flight or Service Definition as it has been offered (and eventually ordered and consumed) in the context of a specific Offer and/or Order." Constraint: "**At time of order, the services should be applied to a single passenger on a single segment.**" Transformation: "At time of Order Creation an Offered Service can become multiple services within the Order Item as the service is broken down per segment and passenger." Attributes include **Status Code** and, separately, **Delivery Status Code**. Associations from Passenger, Delivery Provider, Booking Reference, Validating Carrier, Responsible Airline, Interline Settlement Information. | HIGH |
| M3 | AIDM 24.1 **Commission** | `https://airtechzone.iata.org/aidm_model/24.1/EARoot/EA6/EA1/EA2/EA11/EA11982.htm` | Definition: "A remuneration either an amount of money, or a set percentage of the value involved, **paid to an agent** in relations to a commercial transaction." Attributes (all 0..1): Amount, **Code**, **Commission Code**, Percentage Applied To Amount, Percentage Percent, Remark Text, Taxable Indicator. Associated with Order, Order Item, Offer Item, Interline Settlement Information, Price Quote, Ticket Document Information. | HIGH |
| M4 | AIDM 25.2 **Distribution Chain Role Code** | `https://airtechzone.iata.org/aidm_model/25.2/EARoot/EA6/EA1/EA1/EA2/EA10203.htm` | "The list of allowable roles in a distribution chain": **Carrier** — "An organization which carries the passenger, baggage, or goods, and/or commits to delivering the carriage."; **Distributor** — "An organization that provides a distribution capability such as a certain type of Consolidator, an Aggregator, more generally an intermediary."; **Seller** — "An organization that offers a shopping capability to a shopper." Roles are defined "within the distribution chain mechanism itself, not an entity's primary business classification." | HIGH |
| M5 | AIDM 24.1 **Price** | `https://airtechzone.iata.org/aidm_model/24.1/EARoot/EA6/EA1/EA3/EA1/EA3/EA12368.htm` | Definition: "An amount of money expected, required, or given in payment for something." Attributes include **Base Amount**, **Total Amount**, **Equivalent Amount**, Base Amount Guarantee Time Limit, Masked Indicator, Loyalty Unit Amount/Name. Associations include **Fee, Markup, Tax Summary, Discount, Surcharge, Currency Conversion**, Fare Component, Fare Detail, **Order (Total Price role)**, **Order Item**, Service (Internal Value role). | HIGH |
| M6 | AIDM 25.2 Distribution Chain Link | `https://airtechzone.iata.org/aidm_model/25.2/EARoot/EA6/EA1/EA2/EA9/EA10652.htm` | Not separately retrieved in this run; no claim is made from it. | `NOT RETRIEVED` |

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
| N3 | "Navitaire NDC Gateway" | `https://www.navitaire.com/NDC` | "Obtain seat maps, communicate seat fees and assign or change seat assignments." "Shop for flights, bundles, and flight-related ancillaries, apply promotional codes and more." "Support branded fares with included services (fare families)." Supports "full offer and order capabilities including servicing." | HIGH for the quoted wording — corroborates seat as a merchandised capability distinct from the flight itself |
| N4 | Same page (N3) | as above | The page does **not** name ONE Order, distribution roles or seller/agency designations. | `NOT PUBLICLY PROVEN` |

## Source count

| Group | Revision 1 | Revision 2 |
|---|---|---|
| IATA programme pages | 6 | 7 |
| IATA AIDM entity pages | 0 | **6** (5 retrieved, 1 not retrieved) |
| Amadeus | 2 | 2 |
| Sabre | 4 | 4 |
| Navitaire | 3 | 4 |
| **Total rows** | **15** | **23** (17 carrying a positive claim) |

## How these sources are used

1. As a **semantic** benchmark: do industry-recognised concepts exist in the AeroTech model at all, under any name.
2. Never as a schema to copy. No vendor class name, field name or table is imported.
3. Never to override a recorded AeroTech owner decision. Where a benchmark exposes a concept the owner has not decided on, the matrix raises `BLOCKED_OWNER_CONTRACT` or an `OD-CLOSE-xx`, it does not change semantics.
4. Marketing pages are not used to manufacture a domain concept. Every `EXTERNAL_BENCHMARK_ONLY` row says plainly that it is benchmark context with no S1 obligation.
