# Ledger - semantic facts through outbox, never an Issue gate

Pattern: local Ordering transaction commits facts and Outbox -> publisher -> broker -> Ledger idempotent consumer. Reference transport is the existing broker/consumer framework (AMQP/RabbitMQ where configured); exact production topology/envelope binding is BD-011. No HTTP debit/credit account command is added to Order.

Required fact families: OrderCommercialChangeAccepted, TicketIssued, EmdIssued, TicketVoided, EmdVoided, RefundAuthorized, RefundConfirmed, ExchangeApplied, ExchangeCompleted, ReissueCompleted, SplitTransferCommitted, ServiceDelivered and ServiceDeliveryCorrected. SPEC/events.json defines the event roles/required content.

Envelope: EventId, SchemaVersion, EventType, OwnerAirlineId, OrderId/StreamId, EventOrdinal, CommercialVersion, OrderRevision, FinancialSequence when applicable, OperationId, ChangeId, EconomicFactId, CausationId/CorrelationId and occurred/committed instants. Payload contains authoritative accepted monetary components/scope and source refs sufficient for semantic interpretation; no GL account codes, invented customer balances or live source lookups are necessary to reconstruct the fact.

`EconomicRole` distinguishes CommercialMovement, DocumentEvidence, PaymentEvidence, DeliveryEvidence, Reclassification and WorkflowCompletion; the canonical wire field is economicRole in SPEC/events.json. TicketIssued can reference the original commercial economic fact; ExchangeApplied and ExchangeCompleted share an economic reference but have distinct roles. Ledger chooses what to post and deduplicates monetary recognition by its agreed economic identities; it must not add a document's display amount and the commercial movement again. Ordering never changes a past event payload/ID to correct it; append a correcting fact.

A stable EventId gives delivery dedup; EconomicFactId/line refs prevent double interpretation across different fact events. Delivery consumers also use source consumption identity/portion so duplicate boarded/consumed observations do not create repeated ServiceDelivered facts.

The benchmark Ledger receiver is a durable envelope/economic-reference recorder and duplicate/conflict detector, NOT a general ledger or accounting standards certification. Broker/consumer outage cannot prevent local Issue after its actual capacity/funding/document gates pass. Outbox remains pending and is published later.
