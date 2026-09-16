# Customer and reference data ownership

Customer owns customer/agency/corporate identity and reference details. Ordering owns its accepted financial-customer/buyer/seller snapshots and authorizes each command against trusted actor/customer scope. A passenger is not necessarily the buyer, payer or financial customer. Ordering does not create a second canonical Customer aggregate or customer balance ledger.

Use the existing authenticated context and approved customer/reference snapshot mapping where available. A local reference cache can serve current display and identity lookup; it is not proof that an arbitrary submitted CustomerId is authorized. Missing required authority denies or suspends the use case; no actor0/customer0/default tenant fallback.

AirInfo/BasicInfo provides reference currency/location/operator identity in the reviewed platform context; AirPrice owns price/FX decisions. The new implementation uses only the necessary reference contracts through Application interfaces and ReferenceData/Providers. Exact real wire endpoints/auth mappings must be approved, not guessed from legacy database tables. Reference refresh never recalculates or rewrites accepted currency amounts, buyer snapshots or sold airport/time facts.

GetOrder remains local when Customer/AirInfo/AirPrice are offline. It shows accepted historical context and permission-filtered cached current display separately. Source updates use inbox/entity-version idempotency and do not increment CommercialVersion. Domain receives immutable normalized identity/value snapshots, never an identity service or reference repository to call during a behavior.
