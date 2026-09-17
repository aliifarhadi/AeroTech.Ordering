# Source register and verification scope

Reviewed on 2026-09-16. The attached previous FreshBuild consolidated pack (5053 lines) and all four Agent audit reports were read. Its ZIP membership/integrity was inspected and compared with mounted files; no mismatch was found. Target/owner source facts below were independently inspected. The original v1 pack was sampled for detailed domain/pricing semantics, not exhaustively re-audited in full. No repository was built or live endpoint executed in this design delivery.

The previous reports' owner-side concurrency/expiry claims remain report-derived unless a row below independently verifies the precise claim. Unanswered recommendations in P1-INTEGRATION-A-DECISIONS are not approvals. JetPay local-worktree references from the historical audit are not an authoritative target wire contract.

## SRC-NEW-001

Repository/ref: `aliifarhadi/AeroTech.Ordering@2f2b9033f675387f68ef7ff95c9d0a575d836fb5`.

Path: `AeroTech.Ordering.sln`.

Blob: `bd4075e7e8d83c0abd59413aa23c088df99a7bb0`.

Observed: Selected solution entries inspected; existing shell, not new empty repository.

Source: https://github.com/aliifarhadi/AeroTech.Ordering/blob/2f2b9033f675387f68ef7ff95c9d0a575d836fb5/AeroTech.Ordering.sln

Immutable blob identity: https://api.github.com/repos/aliifarhadi/AeroTech.Ordering/git/blobs/bd4075e7e8d83c0abd59413aa23c088df99a7bb0

## SRC-NEW-002

Repository/ref: `aliifarhadi/AeroTech.Ordering@2f2b9033f675387f68ef7ff95c9d0a575d836fb5`.

Path: `src/AeroTech.Ordering.Domain/AeroTech.Ordering.Domain.csproj`.

Blob: `ba0ede0cbfb9a4388912561474f78f8075a9a977`.

Observed: Domain references generic Core and integration Messages; net10.0.

Source: https://github.com/aliifarhadi/AeroTech.Ordering/blob/2f2b9033f675387f68ef7ff95c9d0a575d836fb5/src/AeroTech.Ordering.Domain/AeroTech.Ordering.Domain.csproj

Immutable blob identity: https://api.github.com/repos/aliifarhadi/AeroTech.Ordering/git/blobs/ba0ede0cbfb9a4388912561474f78f8075a9a977

## SRC-NEW-003

Repository/ref: `aliifarhadi/AeroTech.Ordering@2f2b9033f675387f68ef7ff95c9d0a575d836fb5`.

Path: `src/AeroTech.Ordering.Persistence/OrderingDbContext.cs`.

Blob: `e58b3193dc31250a574045e00520a2894a6219f3`.

Observed: Global decimal18,2 and string256; Outbox/Inbox and CommandDbContext.

Source: https://github.com/aliifarhadi/AeroTech.Ordering/blob/2f2b9033f675387f68ef7ff95c9d0a575d836fb5/src/AeroTech.Ordering.Persistence/OrderingDbContext.cs

Immutable blob identity: https://api.github.com/repos/aliifarhadi/AeroTech.Ordering/git/blobs/e58b3193dc31250a574045e00520a2894a6219f3

## SRC-NEW-004

Repository/ref: `aliifarhadi/AeroTech.Ordering@2f2b9033f675387f68ef7ff95c9d0a575d836fb5`.

Path: `src/AeroTech.Ordering.Consumers/Inbox/InboxConsumeFilter.cs`.

Blob: `72503082820d916cf42205815fb74b900c5d6b25`.

Observed: Missing MessageId bypass; broad DbUpdateException handling; atomicity needs actual runtime certification.

Source: https://github.com/aliifarhadi/AeroTech.Ordering/blob/2f2b9033f675387f68ef7ff95c9d0a575d836fb5/src/AeroTech.Ordering.Consumers/Inbox/InboxConsumeFilter.cs

Immutable blob identity: https://api.github.com/repos/aliifarhadi/AeroTech.Ordering/git/blobs/72503082820d916cf42205815fb74b900c5d6b25

## SRC-NEW-005

Repository/ref: `aliifarhadi/AeroTech.Ordering@2f2b9033f675387f68ef7ff95c9d0a575d836fb5`.

Path: `src/AeroTech.Ordering.Persistence/Inbox/InboxStore.cs`.

Blob: `b1e23e42af9139d6921cfcf5d23be42cdecf6384`.

Observed: Enlist/persist marker behavior inspected; not a proved universal failure.

Source: https://github.com/aliifarhadi/AeroTech.Ordering/blob/2f2b9033f675387f68ef7ff95c9d0a575d836fb5/src/AeroTech.Ordering.Persistence/Inbox/InboxStore.cs

Immutable blob identity: https://api.github.com/repos/aliifarhadi/AeroTech.Ordering/git/blobs/b1e23e42af9139d6921cfcf5d23be42cdecf6384

## SRC-NEW-006

Repository/ref: `aliifarhadi/AeroTech.Ordering@2f2b9033f675387f68ef7ff95c9d0a575d836fb5`.

Path: `src/AeroTech.Ordering.ServiceHost/Program.cs`.

Blob: `c31e61e70fdc73c9b0089a3e4978d1091d2d0f5c`.

Observed: Composition and caller-context location inspected.

Source: https://github.com/aliifarhadi/AeroTech.Ordering/blob/2f2b9033f675387f68ef7ff95c9d0a575d836fb5/src/AeroTech.Ordering.ServiceHost/Program.cs

Immutable blob identity: https://api.github.com/repos/aliifarhadi/AeroTech.Ordering/git/blobs/c31e61e70fdc73c9b0089a3e4978d1091d2d0f5c

## SRC-NEW-007

Repository/ref: `aliifarhadi/AeroTech.Ordering@2f2b9033f675387f68ef7ff95c9d0a575d836fb5`.

Path: `Framework/AeroTech.Framework.Core/AeroTech.Framework.Core.csproj`.

Blob: `b760144708e78e674952c02b6553b53e44be0e5b`.

Observed: Generic net10.0 core project.

Source: https://github.com/aliifarhadi/AeroTech.Ordering/blob/2f2b9033f675387f68ef7ff95c9d0a575d836fb5/Framework/AeroTech.Framework.Core/AeroTech.Framework.Core.csproj

Immutable blob identity: https://api.github.com/repos/aliifarhadi/AeroTech.Ordering/git/blobs/b760144708e78e674952c02b6553b53e44be0e5b


## SRC-NEW-008

Repository/ref: `aliifarhadi/AeroTech.Ordering@957757471d9db3fde91922142d537a559b8084e4` (`k8s-stg`, targeted drift check on 2026-09-17).

Path: `src/AeroTech.Ordering.Providers.Deterministic/DependencyInjection.cs`.

Blob: `c7364d19c42c4c9ff1a0435418ace48a06647743`.

Observed: `AddDeterministicProviders` returns the service collection without registering any business adapter. The project exists as an approved shell/composition seam; working reservation/funding/document/etc. deterministic adapters must be implemented by the relevant slice rather than assumed. A directory listing on the same branch showed only the project file, DependencyInjection, options and activation plumbing.

Source: https://github.com/aliifarhadi/AeroTech.Ordering/blob/957757471d9db3fde91922142d537a559b8084e4/src/AeroTech.Ordering.Providers.Deterministic/DependencyInjection.cs

Immutable blob identity: https://api.github.com/repos/aliifarhadi/AeroTech.Ordering/git/blobs/c7364d19c42c4c9ff1a0435418ace48a06647743

## SRC-OLD-001

Repository/ref: `aliifarhadi/Ordering@077a851217ab0fc3bce922973bb3c7f76e6f2f7f`.

Path: `docs/modern-pss-order-domain-design-v1-Latest/order-domain-design-v1/01-DOMAIN-MODEL.md`.

Blob: `287c831729eeb05c1b4fd1b6b97cfadba30c0578`.

Observed: Selected domain sections through ~1500 lines; later response truncated; not a claim of full original pack audit.

Source: https://github.com/aliifarhadi/Ordering/blob/077a851217ab0fc3bce922973bb3c7f76e6f2f7f/docs/modern-pss-order-domain-design-v1-Latest/order-domain-design-v1/01-DOMAIN-MODEL.md

Immutable blob identity: https://api.github.com/repos/aliifarhadi/Ordering/git/blobs/287c831729eeb05c1b4fd1b6b97cfadba30c0578

## SRC-OLD-002

Repository/ref: `aliifarhadi/Ordering@077a851217ab0fc3bce922973bb3c7f76e6f2f7f`.

Path: `docs/modern-pss-order-domain-design-v1-Latest/order-domain-design-v1/02-PRICING-AND-SERVICING.md`.

Blob: `59435dfc05215a88a1a4b4b9d267addff87d1b87`.

Observed: Lines1..330: explicit sign/effect, line-role, allocation/FX and customer/guarantee distinctions.

Source: https://github.com/aliifarhadi/Ordering/blob/077a851217ab0fc3bce922973bb3c7f76e6f2f7f/docs/modern-pss-order-domain-design-v1-Latest/order-domain-design-v1/02-PRICING-AND-SERVICING.md

Immutable blob identity: https://api.github.com/repos/aliifarhadi/Ordering/git/blobs/59435dfc05215a88a1a4b4b9d267addff87d1b87

## SRC-AO-001

Repository/ref: `aliifarhadi/Aerotech.AirOffer@k8s-stg`.

Path: `AeroTech.AirAvail/AirOffer/src/AeroTech.AirOffer.RestApi/V1/AirOffers/Controllers/Service2Service/ServiceController.cs`.

Blob: `6537f90933473faae4dbab000d7ff924802e748b`.

Observed: Observed service Details route and BaseResult wrapper.

Source: https://github.com/aliifarhadi/Aerotech.AirOffer/blob/k8s-stg/AeroTech.AirAvail/AirOffer/src/AeroTech.AirOffer.RestApi/V1/AirOffers/Controllers/Service2Service/ServiceController.cs

Immutable blob identity: https://api.github.com/repos/aliifarhadi/Aerotech.AirOffer/git/blobs/6537f90933473faae4dbab000d7ff924802e748b

## SRC-AO-002

Repository/ref: `aliifarhadi/Aerotech.AirOffer@k8s-stg`.

Path: `AeroTech.AirAvail/AirOffer/src/AeroTech.AirOffer.Services/Services/Service2Service/ServiceFlightOfferDetailService.cs`.

Blob: `75144d5f4bf096e4910777c73fcfbc7aa76d4704`.

Observed: Decoded offer context and actual PriceAsync call; current Details is not immutable accepted snapshot read-back.

Source: https://github.com/aliifarhadi/Aerotech.AirOffer/blob/k8s-stg/AeroTech.AirAvail/AirOffer/src/AeroTech.AirOffer.Services/Services/Service2Service/ServiceFlightOfferDetailService.cs

Immutable blob identity: https://api.github.com/repos/aliifarhadi/Aerotech.AirOffer/git/blobs/75144d5f4bf096e4910777c73fcfbc7aa76d4704

## SRC-AO-003

Repository/ref: `aliifarhadi/Aerotech.AirOffer@k8s-stg`.

Path: `AeroTech.AirAvail/AirOffer/src/AeroTech.AirOffer.Services/Dto/Service2Service/ServiceFlightOfferDetailDto.cs`.

Blob: `adb3bca9705d443273787e864c237b1d7552ed92`.

Observed: Service DTO inspected; priced projection is not issued ticket/document proof.

Source: https://github.com/aliifarhadi/Aerotech.AirOffer/blob/k8s-stg/AeroTech.AirAvail/AirOffer/src/AeroTech.AirOffer.Services/Dto/Service2Service/ServiceFlightOfferDetailDto.cs

Immutable blob identity: https://api.github.com/repos/aliifarhadi/Aerotech.AirOffer/git/blobs/adb3bca9705d443273787e864c237b1d7552ed92

## SRC-AO-004

Repository/ref: `aliifarhadi/Aerotech.AirOffer@k8s-stg`.

Path: `AeroTech.AirAvail/AirOffer/src/AeroTech.AirOffer.Services/Dto/Service2Service/ServiceGetFlightOffersDetailRequest.cs`.

Blob: `00885a3bbe2174cf8afbb670733486f01eaa4ef9`.

Observed: Request carries opaque OfferId.

Source: https://github.com/aliifarhadi/Aerotech.AirOffer/blob/k8s-stg/AeroTech.AirAvail/AirOffer/src/AeroTech.AirOffer.Services/Dto/Service2Service/ServiceGetFlightOffersDetailRequest.cs

Immutable blob identity: https://api.github.com/repos/aliifarhadi/Aerotech.AirOffer/git/blobs/00885a3bbe2174cf8afbb670733486f01eaa4ef9

## SRC-AO-005

Repository/ref: `aliifarhadi/Aerotech.AirOffer@k8s-stg`.

Path: `AeroTech.AirAvail/Framework/AeroTech.Framework.Presentation/AspNetCore/Responses/BaseResult.cs`.

Blob: `bc23267bd35cbf2ed855386182b8b42d163c75a8`.

Observed: CLR Data/Errors wrapper; deployed serializer not tested.

Source: https://github.com/aliifarhadi/Aerotech.AirOffer/blob/k8s-stg/AeroTech.AirAvail/Framework/AeroTech.Framework.Presentation/AspNetCore/Responses/BaseResult.cs

Immutable blob identity: https://api.github.com/repos/aliifarhadi/Aerotech.AirOffer/git/blobs/bc23267bd35cbf2ed855386182b8b42d163c75a8

## SRC-AO-006

Repository/ref: `aliifarhadi/Aerotech.AirOffer@k8s-stg`.

Path: `AeroTech.AirAvail/Contracts/AeroTech.Messages/AirOffer/Enums/ServiceOfferPricingCategory.cs`.

Blob: `5b7d10f1ef0515526bae53dfab13b9332c453808`.

Observed: Fare, Tax, Fee, Surcharge enum.

Source: https://github.com/aliifarhadi/Aerotech.AirOffer/blob/k8s-stg/AeroTech.AirAvail/Contracts/AeroTech.Messages/AirOffer/Enums/ServiceOfferPricingCategory.cs

Immutable blob identity: https://api.github.com/repos/aliifarhadi/Aerotech.AirOffer/git/blobs/5b7d10f1ef0515526bae53dfab13b9332c453808

## SRC-AP-001

Repository/ref: `aliifarhadi/Aerotech.AirPrice@k8s-stg`.

Path: `AeroTech.AirPrice/AirPrice/src/AeroTech.AirPrice.RestApi/V1/AirFareAggregate/Controllers/ServiceController.cs`.

Blob: `02383f31c83dfd43a2f07e9a13e0c3a971b4dbe7`.

Observed: Observed fare queries and BoundReservationValidation/BoundTicketingValidation; not an exhaustive whole-owner capability audit.

Source: https://github.com/aliifarhadi/Aerotech.AirPrice/blob/k8s-stg/AeroTech.AirPrice/AirPrice/src/AeroTech.AirPrice.RestApi/V1/AirFareAggregate/Controllers/ServiceController.cs

Immutable blob identity: https://api.github.com/repos/aliifarhadi/Aerotech.AirPrice/git/blobs/02383f31c83dfd43a2f07e9a13e0c3a971b4dbe7

## SRC-FF-001

Repository/ref: `aliifarhadi/Aerotech.FlightFlow@k8s-stg`.

Path: `AeroTech.FlightFlow/FlightFlow/src/AeroTech.FlightFlow.RestApi/V1/FlightAggregate/Controllers/ServiceController.cs`.

Blob: `1d383996fb6231b8ea47664425c3620f6d200c82`.

Observed: Mutation route response shapes, commented hold GET and flight/version reads.

Source: https://github.com/aliifarhadi/Aerotech.FlightFlow/blob/k8s-stg/AeroTech.FlightFlow/FlightFlow/src/AeroTech.FlightFlow.RestApi/V1/FlightAggregate/Controllers/ServiceController.cs

Immutable blob identity: https://api.github.com/repos/aliifarhadi/Aerotech.FlightFlow/git/blobs/1d383996fb6231b8ea47664425c3620f6d200c82

## SRC-FF-002

Repository/ref: `aliifarhadi/Aerotech.FlightFlow@k8s-stg`.

Path: `AeroTech.FlightFlow/FlightFlow/src/AeroTech.FlightFlow.RestApi/V1/FlightAggregate/Requests/HoldSeatsRequest.cs`.

Blob: `9a52958399f87ece7dd6111bb0f65ff5a095748c`.

Observed: Actual expiry/passenger/revenue/allotment/reference request fields.

Source: https://github.com/aliifarhadi/Aerotech.FlightFlow/blob/k8s-stg/AeroTech.FlightFlow/FlightFlow/src/AeroTech.FlightFlow.RestApi/V1/FlightAggregate/Requests/HoldSeatsRequest.cs

Immutable blob identity: https://api.github.com/repos/aliifarhadi/Aerotech.FlightFlow/git/blobs/9a52958399f87ece7dd6111bb0f65ff5a095748c

## SRC-NEW-009

Repository/ref: `aliifarhadi/AeroTech.Ordering@k8s-stg`.

Path: `src/AeroTech.Ordering.Domain/_Shared/Contracts/ICallerContext.cs`.

Blob: `a55f3ca634b8159a0bbf76ccf2d028235d0e3f52`.

Observed: Domain caller-context contract uses `BusinessContextType`, `PrincipalType` and `AuthorizationSurface` from platform Messages enum namespaces; no owner-airline field is present.

Source: https://github.com/aliifarhadi/AeroTech.Ordering/blob/k8s-stg/src/AeroTech.Ordering.Domain/_Shared/Contracts/ICallerContext.cs

Immutable blob identity: https://api.github.com/repos/aliifarhadi/AeroTech.Ordering/git/blobs/a55f3ca634b8159a0bbf76ccf2d028235d0e3f52

## SRC-NEW-010

Repository/ref: `aliifarhadi/AeroTech.Ordering@k8s-stg`.

Path: `src/AeroTech.Ordering.Domain/_Shared/Contracts/IHomeOperatorProvider.cs`.

Blob: `37371011678f9e5f7494ad5d69a59758772343d2`.

Observed: Trusted owner-airline seam is `GetOwnerAirlineIdAsync`; it is separate from caller context.

Source: https://github.com/aliifarhadi/AeroTech.Ordering/blob/k8s-stg/src/AeroTech.Ordering.Domain/_Shared/Contracts/IHomeOperatorProvider.cs

Immutable blob identity: https://api.github.com/repos/aliifarhadi/AeroTech.Ordering/git/blobs/37371011678f9e5f7494ad5d69a59758772343d2

## SRC-NEW-011

Repository/ref: `aliifarhadi/AeroTech.Ordering@k8s-stg`.

Path: `src/AeroTech.Ordering.ServiceHost/OperatorContext/ReferenceDataHomeOperatorProvider.cs`.

Blob: `21a0ed1afd2c1e57d5d1977af8f52a4d45f69fbb`.

Observed: Current ServiceHost implementation resolves HomeAirlineId from ReferenceData operator settings and rejects missing/invalid home-operator provisioning.

Source: https://github.com/aliifarhadi/AeroTech.Ordering/blob/k8s-stg/src/AeroTech.Ordering.ServiceHost/OperatorContext/ReferenceDataHomeOperatorProvider.cs

Immutable blob identity: https://api.github.com/repos/aliifarhadi/AeroTech.Ordering/git/blobs/21a0ed1afd2c1e57d5d1977af8f52a4d45f69fbb

## Attached artifact fingerprints

- `ORDERING-VNEXT-FRESHBUILD-DESIGN-PACK.md`: SHA256 `db7a996f0caadc301322ef1ecd59ad8f693ae88b6d35f1fd0cda81c952bcc7bb`; 208186 bytes; Complete text review.
- `ORDERING-VNEXT-FRESHBUILD-DESIGN-PACK.zip`: SHA256 `e27e52f418d2cc934d392373cc6b40a66b245bcfb53c5cce2c0a10834a50a1dd`; 110894 bytes; ZIP member/integrity and mounted-file comparison.
- `P1-RUNTIME-PATH-MAP(1).md`: SHA256 `5951f7c77057b5506ac2d622340d509878eba70a94529476fcbf110fbb1334c5`; 23695 bytes; Complete text review.
- `P1-INTEGRATION-GAP-MAP(1).md`: SHA256 `1df7c658bc68038335c10c32ff5ef5a22fef4bc92a3674fadf8790f93dff7690`; 19205 bytes; Complete text review.
- `P1-INTEGRATION-A-DECISIONS(1).md`: SHA256 `933bb435b179135c65dabc7e32908c46ec3626e7877ae7b4ec09a0d3af26dc21`; 14566 bytes; Complete text review.
- `P1-CUTOVER-PLAN(1).md`: SHA256 `63720ebde98441744a0aee2967b3e4d9d6a7cc554a1b42fda4ac688acabc1312`; 10501 bytes; Complete text review.

All target design additions are identified by TARGET_DECISION or OWNER_REQUIRED. This source register is provenance, not permission to copy old business implementation or demand access to these repositories during fresh domain implementation.
