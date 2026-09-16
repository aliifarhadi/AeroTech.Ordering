# Common semantic port contract

## Scope

Port types live in Application. A wire DTO lives in Providers/<Owner>/Wire or Messages. A simulator implements the SAME semantic port/result invariants; its own HTTP routes are test infrastructure, not invented production owner endpoints.

## Mutation request

Required common fields: `ContractVersion`, `OwnerAirlineId`, `OperationId`, `ExternalOperationId`, `ProviderProfileId`, stable `OperationKey`, `RequestHash`, exact `TargetScope`, expected source/obligation/control versions where relevant, initiating authority reference, and a transport deadline budget. Typed payload adds the requested business effect. RequestHash canonicalization is versioned and deterministic. The server must bind a key to one semantic payload and reject conflicting reuse.

Request timeout/deadline is not a resource TTL. Hold expiry is owner-returned; requested duration/deadline is only a request unless the owner contract explicitly makes it authoritative. Never use a retry deadline to generate an OfferExpiresAt.

## Result and evidence

Each target result contains Outcome (Confirmed/Rejected/Pending/Unknown), target identity, owner operation/resource/member refs as relevant, applied effect, exact scope/value/currency, source revision/timestamps, expiry/validity facts where relevant, raw evidence reference/hash and reason code. Confirmed cannot be constructed by copying missing fields from the request. Rejected must be definitive no-effect for that target; otherwise record partial/unknown evidence.

For batches retain a result for every requested member (explicit Unknown when response lacks one), additional unexpected members as ContractMismatch evidence, derived summary and HasUnknown. A truncated response is not full success. Repeated evidence with same identity/hash is no-op; identity with changed payload is conflict.

## Read contracts

ReadOperation accepts original operation key and owner/profile/scope, returning the effect's history. ReadResource accepts canonical resource reference and returns its current state/revision. Read-back is side-effect-free. Outcome `Absent` is separate from Rejected and actionable only when `Authoritative=true`, exact lookup scope, retention/consistency assurance and source observation time are supplied. `NotVisibleYet`, `KeyExpired` or incomplete read is Unknown.

## Transport and retries

Reference sync integrations use HTTPS/JSON with explicit authentication. Commands can return terminal result or Pending; Ordering returns a local OperationId and resumes asynchronously when its wait budget ends. A provider event/webhook is an optional source of evidence, not mandatory for the reference path; it enters through authenticated inbox and the same evidence validator.

Safe queries use bounded retries within the caller's deadline. Mutation requests are never blindly retried by a generic HTTP resilience policy. Recovery may replay the saved mutation under its original key only after capability certification. 429/503/timeout after possible dispatch does not prove no effect. Pending poll instructions are advisory within bounded backoff, not permission to mutate business TTL.

## Capability certification descriptor

Each binding supplies `ProfileId`, `EnvironmentClass`, `Owner`, `ContractVersion`, `SupportedOperations`, `IdempotencyScope`, `KeyRetentionPolicy`, `SameKeyDifferentPayloadBehavior`, `ReadBackConsistency`, `ResourceIdentityNamespace`, `PartialResultModel`, `ExpiryAuthority`, `CompensationCapabilities`, `ClockSemantics`, `AuthenticationScheme` and `CertificationEvidenceRef`.

Profile selection happens at the composition boundary; persist it on every operation/resource. Domain/Application never branch on Real versus Simulator, but may reason about semantic capabilities/authority explicitly present in the profile. Unsupported capabilities fail before new dispatch. A simulator's certificate only attests target-contract tests, never the actual JetPay/FlightFlow server.
