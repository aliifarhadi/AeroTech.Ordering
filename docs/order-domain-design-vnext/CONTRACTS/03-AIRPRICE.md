# AirPrice - authoritative pricing/servicing decisions

Owner: AirPrice. Required semantic interface `IPricingDecisionPort` exposes QuoteCancellation, QuoteRefund, QuoteExchange, QuoteAncillary, ValidateAcceptedDecision and ReadDecision when supported. These are semantic queries; a provider may persist its own quote artifact, but it must not mutate Order, reserve inventory or execute payment. Target HTTPS endpoints belong to `/service/`; exact unverified owner routes remain unbound, not invented.

Every decision supplies DecisionId/version, source pricing context/rule references, original sale pricing date, affected OrderItems/Services/PricingUnits/travelers, complete old/new scope, component magnitudes/currencies/effects/directions or explicitly labeled differentials, original line/occurrence refs, approved allocations where required, penalties/waivers/authority, additional collection/residual/refund disposition, validity and a binding digest or equivalent immutable identity.

Pricing computes used fare, refundable taxes, no-show penalty, discounts and currency conversion. Ordering checks scope/version/total conservation and records the accepted result; it does not reconstruct rules from boolean summaries or today's catalog. Missing historical context returns PricingContextUnavailable; a new quote requires new explicit acceptance.

Observed service controller exposes `GET /Service/v1/AirFares/{airFareId}`, `GET /Service/v1/AirFares/ForTicketing`, `POST /Service/v1/BoundReservationValidation` and `POST /Service/v1/BoundTicketingValidation` (plus the controller's base GET). These inspected query routes are NOT proof of a complete refund/exchange quote contract. Mapping them to a target pricing decision without schema/semantic evidence is forbidden. The current controller uses ApiResult wrappers, not necessarily AirOffer's BaseResult.

BD-007 covers real servicing decision/version/history bindings. Reference simulators store immutable decisions with exact valid-through rules and deterministic synthetic outcomes. They MUST change decision identity/version when changing amount/scope; a retry of a bound decision never silently returns repriced content. Test cases include true RT vs two OW, tax occurrences, included benefit, used portion, waived penalty, expired quote, widening scope and original-currency partial reversal.

No price call is introduced into accepted Create or GetOrder. A before-acceptance preparation may ask pricing through the prescribed port or AirOffer's observed pipeline; that source fact is labeled honestly.
