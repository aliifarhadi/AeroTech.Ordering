# Required acceptance scenarios

These are test specifications, not passed test reports. Every scenario needs an actual test/result in the stage evidence. Artificial fixtures are not live owner facts.

## SC-D0-001 - Correct repository and preserved worktree

**Given:** Target checkout contains bootstrap plus an unrelated local change.

**When:** Run the day-zero inventory.

**Then:** Remote/HEAD and diff are recorded; no reset, deletion, recopy or new solution.

Invariants: INV-001.

## SC-D0-002 - Read source findings before copying

**Given:** Reviewed old pack and reports are available in this pack as findings.

**When:** Produce layer/file dispositions.

**Then:** Every observed shell discrepancy has one named B0 fix; old reports are not assumed runtime proof.

Invariants: INV-001, INV-056.

## SC-B0-001 - Forbidden dependency

**Given:** Domain.csproj references Messages.

**When:** Run architecture suite.

**Then:** Test fails before business implementation; remove dependency through explicit B0 change.

Invariants: INV-002.

## SC-B0-002 - Decimal round-trip

**Given:** SQL mapping contains values 1.23456789 and rate 0.123456789123.

**When:** Save/reload after migration.

**Then:** Exact decimal equality; no global18,2 loss.

Invariants: INV-059.

## SC-B0-003 - Duplicate DI

**Given:** Two IUnitOfWork or two real/sim bindings exist.

**When:** Build provider profile.

**Then:** Startup/architecture test fails with named duplicate.

Invariants: INV-003.

## SC-B0-004 - Inbox missing source ID

**Given:** Broker message lacks required source identity.

**When:** Receive it.

**Then:** Reject/quarantine; do not bypass dedup.

Invariants: INV-004.

## SC-B0-005 - Inbox business crash

**Given:** Inbox and normalized work are enlisted.

**When:** Crash before commit then redeliver.

**Then:** Both absent first; one durable acceptance after retry.

Invariants: INV-004.

## SC-B0-006 - Unrelated SQL failure

**Given:** Consumer SaveChanges violates unrelated constraint.

**When:** Handle DbUpdateException.

**Then:** Failure propagates; not discarded as duplicate.

Invariants: INV-004.

## SC-B0-007 - Outbox broker response loss

**Given:** Fact committed; broker accepts and response is lost.

**When:** Restart and republish.

**Then:** Same EventId/payload; receiver commits one fact.

Invariants: INV-005.

## SC-B0-008 - Independent owner persistence

**Given:** Persistent simulator state and Ordering SQL are separate.

**When:** Commit owner effect then crash Ordering before local evidence.

**Then:** Owner effect survives and can be read back independently.

Invariants: INV-019, INV-020, INV-056.

## SC-B0-009 - Target-only startup

**Given:** Old Ordering repository and services are inaccessible.

**When:** Restore/build/start target with local dependencies.

**Then:** No old business assembly/path/database/process is required.

Invariants: INV-056.

## SC-B0-010 - Host auth/readiness

**Given:** Empty host and disposable SQL exist.

**When:** Call liveness, readiness, authenticated diagnostics then restart.

**Then:** Liveness independent of provider outage; readiness truthful; invalid credentials denied.

Invariants: INV-002, INV-017.

## SC-S1-001 - Real candidate repriced before acceptance

**Given:** Real priced offer ID; Details can return changed price.

**When:** Prepare then show/accept stored digest; Create.

**Then:** Order preserves prepared amount exactly; zero owner calls during Create.

Invariants: INV-006, INV-018.

## SC-S1-002 - Authoritative simulator Create

**Given:** Prepared package 1 traveler/1 segment with fare100+tax20.

**When:** Accept and Create.

**Then:** One item, one service, no document/reservation/payment; total120, CV1/FinancialSequence1.

Invariants: INV-006, INV-009, INV-012, INV-054.

## SC-S1-003 - Replay after owner outage

**Given:** Created order and terminal receipt; AirOffer offline.

**When:** Replay original Create.

**Then:** Same status/order IDs with replay flag; no network access.

Invariants: INV-007, INV-016.

## SC-S1-004 - Key conflict

**Given:** Create key already used for preparationA.

**When:** Submit same key for preparationB.

**Then:** 409; original facts untouched.

Invariants: INV-007.

## SC-S1-005 - Concurrent different keys same preparation

**Given:** Two clients accept the same snapshot.

**When:** Race two Create requests.

**Then:** One Order commits; other gets PreparationAlreadyConsumed; no partial read model.

Invariants: INV-008.

## SC-S1-006 - Atomic projection failure

**Given:** Order insert succeeds inside transaction.

**When:** Inject projection failure before commit.

**Then:** No Order, price, outbox or receipt-success becomes visible.

Invariants: INV-003, INV-016.

## SC-S1-007 - Technical-stop flight

**Given:** One passenger segment contains 2 physical legs.

**When:** Create and read.

**Then:** One air service; two itinerary legs; no fabricated two coupons.

Invariants: INV-010.

## SC-S1-008 - Source-priced package

**Given:** Two travelers, two segments in one accepted package.

**When:** Create.

**Then:** One item may contain four services; ticket display rows do not force independent items.

Invariants: INV-009, INV-011.

## SC-S1-009 - Different fare construction

**Given:** Same round-trip itinerary provided once as RT PU and once as 2OW PUs.

**When:** Normalize and persist both.

**Then:** Different source pricing structures retained; no route-shape inference.

Invariants: INV-015.

## SC-S1-010 - Opaque construction

**Given:** Source lacks reliable item/fare component mappings.

**When:** Normalize current AirOffer projection.

**Then:** Conservative package and opaque context; no invented PU/FC links.

Invariants: INV-009, INV-015.

## SC-S1-011 - Double quantity

**Given:** Source line already extends 2 adults at total200.

**When:** Normalize pricing group quantity2.

**Then:** Store200, not400; preserve quantity metadata.

Invariants: INV-012.

## SC-S1-012 - Currency equivalents

**Given:** Original fare100 USD; authoritative sale92 EUR.

**When:** Create in EUR.

**Then:** Customer value92; original100 retained; do not add192 or recompute FX.

Invariants: INV-012, INV-055, INV-059.

## SC-S1-013 - Sign and settlement

**Given:** Fare400 + bag50 + discount credit45 + settlement commission20.

**When:** Compute totals and reverse discount45.

**Then:** Customer405 initially,450 after explicit reversal; commission excluded.

Invariants: INV-013.

## SC-S1-014 - Allocation not money

**Given:** Price line100 with allocations60/40.

**When:** Compute payable and second purpose view.

**Then:** Total100, not200; purposes/versions never combined.

Invariants: INV-014.

## SC-S1-015 - Source hierarchy totals

**Given:** Root120 but coupon sum119 with no supplied residual.

**When:** Normalize/Create.

**Then:** ContractMismatch; no synthetic rounding fee.

Invariants: INV-012, INV-055.

## SC-S1-016 - Invalid traveler binding

**Given:** Offer has travelerrefsA/B but request maps A twice.

**When:** Create.

**Then:** 422 before any Order; no index-based guessing.

Invariants: INV-011, INV-017.

## SC-S1-017 - Cross-customer replay

**Given:** CustomerB knows CustomerA key/orderID.

**When:** Replay/Get.

**Then:** Denied or undisclosed404, no result leak.

Invariants: INV-017.

## SC-S1-018 - Missing validity

**Given:** Live candidate lacks offer/price validity.

**When:** Prepare sandbox and attempt real-effect profile.

**Then:** Show NotSupplied and LIVE_ACCEPTANCE_BLOCKED; no false infinite validity.

Invariants: INV-018, INV-055.

## SC-S1-019 - Independent deadlines

**Given:** OfferT+10, priceT+5, holdT+20, no authoritativeticketdeadline.

**When:** Read facts and expire clock.

**Then:** Keep independent owners; earliest display is not one stored Order TTL.

Invariants: INV-055.

## SC-S1-020 - Restart and rebuild

**Given:** Created Order with durable model; dependencies offline.

**When:** Restart, delete only test projection, run authorized rebuild.

**Then:** Same commercial values/IDs; no owner requests; no new commercial version.

Invariants: INV-016, INV-054.

## SC-S1-021 - Unknown registered product

**Given:** Source contains unregistered schema/version.

**When:** Prepare/Create.

**Then:** UnsupportedCapability before accepting sale.

Invariants: INV-058.

## SC-S2-001 - Hold has durable intent

**Given:** Active authoritative Order needs1seat.

**When:** Reserve with saved effect identity.

**Then:** Intent committed before provider dispatch; exact member/profile/hash persisted.

Invariants: INV-019, INV-023.

## SC-S2-002 - Unknown replay reaches recovery

**Given:** Owner holds seat but reply lost.

**When:** Replay same client key after restart.

**Then:** Recover same operation, not AlreadyReserved and not another hold.

Invariants: INV-007, INV-020.

## SC-S2-003 - Rejected can be retried explicitly

**Given:** Owner definitively rejected prior reserve with no resource.

**When:** New accepted Reserve key after terminal operation.

**Then:** New operation allowed; old rejection retained.

Invariants: INV-020, INV-022.

## SC-S2-004 - Married partial results

**Given:** 2-member married group returns1Held+1Unknown.

**When:** Apply evidence then request Issue.

**Then:** No full confirmed rollup/Issue; unresolved member recovered; successful member not duplicated.

Invariants: INV-022, INV-029.

## SC-S2-005 - All-required-or-release

**Given:** Independent batch1Held+1Rejected.

**When:** Execute reference failure disposition.

**Then:** Confirmed hold released via separate durable effect; commercial sale history remains.

Invariants: INV-022, INV-024.

## SC-S2-006 - Infant explicit units

**Given:** Adult seat1, lap-infant seat0, seated-infant seat1.

**When:** Build required capacity request.

**Then:** 2 seat units, not one seat per traveler or exclude every infant.

Invariants: INV-023.

## SC-S2-007 - Owner expiry vs clock

**Given:** Held capacity validUntilT.

**When:** Advance clock pastT; hold read-back unavailable.

**Then:** Eligibility expires, but resource release remains unknown; no silent owner release.

Invariants: INV-024, INV-055.

## SC-S2-008 - 204 is not confirmation

**Given:** Adapter receives204 on missing/unproven hold.

**When:** Map commit/release result.

**Then:** Unknown/contract gap unless certified evidence proves effect; no fabricated confirmation.

Invariants: INV-020, INV-024.

## SC-S2-009 - Stale worker

**Given:** WorkerA lease expires; B resumes same operation.

**When:** A returns with delayed outcome and tries next mutation.

**Then:** Evidence retained, fence blocks A dispatch/advance; claim survives.

Invariants: INV-021.

## SC-S2-010 - Exact request replay

**Given:** Provider intent persisted; unrelated source data changes.

**When:** Recover original request.

**Then:** Saved member payload/key/profile unchanged; not rebuilt from current Order.

Invariants: INV-019, INV-020.

## SC-S2-011 - NotFound retention gap

**Given:** Owner forgets idempotency key after retention.

**When:** Read operation returnsnotfound.

**Then:** Unknown/manual reconciliation, not permission to create another effect.

Invariants: INV-020.

## SC-S2-012 - Release lost response

**Given:** Owner released held seat.

**When:** Restart before local result and resume.

**Then:** One release; original operation read-back resolves; no reacquisition.

Invariants: INV-019, INV-020, INV-024.

## SC-S3-001 - Exact coverage

**Given:** Obligation120 EUR version1.

**When:** Establish confirmed coverage120 EUR with ownerref.

**Then:** Evidence usable only for exact obligation/scope.

Invariants: INV-025.

## SC-S3-002 - Wrong currency

**Given:** 120 EUR due; result120 USD.

**When:** Apply evidence.

**Then:** ContractMismatch; Issue authority denied despite equal numeric amount.

Invariants: INV-025.

## SC-S3-003 - No canonical reference

**Given:** ResultConfirmed120 but owner applicationrefmissing.

**When:** Validate result.

**Then:** Not accepted as usable evidence.

Invariants: INV-025.

## SC-S3-004 - Guarantee not cash

**Given:** Coverage guarantee120 with no payment capture.

**When:** Read payment facet.

**Then:** CashApplied0, GuaranteedCoverage120; no fabricated capture.

Invariants: INV-026.

## SC-S3-005 - Mixed funding

**Given:** Cash80 and guarantee40 for same120 obligation.

**When:** Read coverage and settle guarantee later.

**Then:** Covered120 not160/200; settlement supersedes guarantee without double application.

Invariants: INV-026.

## SC-S3-006 - Funding Unknown

**Given:** Owner establishes coverage, response lost.

**When:** Restart and recover.

**Then:** One owner effect; Issue remains pending until authoritative evidence.

Invariants: INV-020, INV-025.

## SC-S3-007 - Check-use race

**Given:** Coverage query valid then expires before issue.

**When:** Acquire operation-bound IssueAuthority.

**Then:** No issue from stale query; acquire authority or suspend.

Invariants: INV-027.

## SC-S3-008 - New bag delta

**Given:** Original120 fulfilled; new bag20 accepted.

**When:** Fund new obligation.

**Then:** Request20 for new scope; no second120 capture.

Invariants: INV-028.

## SC-S3-009 - Authority release after abort

**Given:** No document pivot happened; issue authority acquired.

**When:** Abort/release with response loss.

**Then:** Same authority released/read back; funds cannot be reused while unknown.

Invariants: INV-020, INV-027.

## SC-S3-010 - Provider change during recovery

**Given:** Operation pinned to simulatorprofileA.

**When:** Change default config to profileB.

**Then:** Existing operation resumes A; new provider cannot impersonate old evidence.

Invariants: INV-019, INV-025.

## SC-S4-001 - Hold starts Issue but cannot finalize

**Given:** Valid Held capacity and acceptable coverage plan.

**When:** Issue.

**Then:** CanStartIssue true; commit capacity and acquire funding authority; only then write documents.

Invariants: INV-029.

## SC-S4-002 - Commit Unknown blocks documents

**Given:** FlightFlow commits but response lost.

**When:** Resume Issue.

**Then:** No document yet; same commit read-back resolves then issue once.

Invariants: INV-020, INV-029.

## SC-S4-003 - Local atomic document set

**Given:** Two travelers; valid all-required gate and stock.

**When:** Crash during second local ticket creation before commit.

**Then:** Zero committed documents; no first partial local ticket; retry uses coherent transaction.

Invariants: INV-030, INV-032.

## SC-S4-004 - Stock allocation race

**Given:** Two workers allocate same namespace/range.

**When:** Concurrent Issue.

**Then:** Unique numbers under SQL concurrency; no duplicate or random replacement.

Invariants: INV-031.

## SC-S4-005 - Overlapping ranges

**Given:** Active namespace range1000..2000.

**When:** Concurrently register1500..2500.

**Then:** One conflicting registration rejected atomically.

Invariants: INV-031.

## SC-S4-006 - External issue response loss

**Given:** External issuer returns after issuing assignednumber.

**When:** Crash before evidence save then recover.

**Then:** Same assignednumber/key and one actual document.

Invariants: INV-020, INV-031, INV-032.

## SC-S4-007 - External partial traveler set

**Given:** External issuer issuedtravelerA, travelerBUnknown.

**When:** Apply evidence and cancel/retry.

**Then:** A retained; B recovered same operation; no whole-order fullyissued summary.

Invariants: INV-030, INV-032.

## SC-S4-008 - EMD purpose core

**Given:** ServiceEMD plus fee-onlyEMDS.

**When:** Issue with appropriate references.

**Then:** Service references stable service; fee references pricingline, no fake air service.

Invariants: INV-030, INV-038.

## SC-S4-009 - Ledger offline

**Given:** All issue gates met, broker unavailable.

**When:** Commit local ETKT then retry delivery.

**Then:** Issue succeeds locally; same outboxEventId pending; Ledger not a gate.

Invariants: INV-033, INV-005.

## SC-S4-010 - Independent coupon axes

**Given:** Local open coupon with external controlpending.

**When:** TryVoid/Refund/Issue-relatedservicing.

**Then:** Control uncertainty is explicit; not replaced by one generic coupon status.

Invariants: INV-030, INV-039.

## SC-S4-011 - Expiry after timely confirmation

**Given:** Owner effect committed before validdeadline, replyarrivesafter.

**When:** Apply recovered authoritative event.

**Then:** Retain timely valid result; no destructive expiry overwrite.

Invariants: INV-024, INV-029.

## SC-S5-001 - Cancel unissued whole scope

**Given:** Held/funded Order with no documents.

**When:** Accept exact cancellationplan.

**Then:** Independent requiredreleases resolve then one commercialcancel pivot.

Invariants: INV-034, INV-054.

## SC-S5-002 - Mixed release unknown

**Given:** Capacityreleased; fundingreleaseUnknown.

**When:** Retry cancellation.

**Then:** No finalcancel and no capacityre-reserve; same fundingrelease recovery.

Invariants: INV-020, INV-034.

## SC-S5-003 - Issued scope guard

**Given:** At least one requestedservice has issuedcoupon.

**When:** Pre-ticketCancel.

**Then:** Reject or require explicit permittednewplan; never silentlyVoid/Refund.

Invariants: INV-034.

## SC-S5-004 - No external resources

**Given:** Created unreservedunfunded Order.

**When:** Accept cancellation.

**Then:** No owner calls; local lineage/cancel+price treatment atomic.

Invariants: INV-034.

## SC-S5-005 - Scope with dependent seat

**Given:** Unissued air and paid seat dependent.

**When:** Cancel only air request.

**Then:** Return expandeddependentdisposition for explicit acceptance; no dangling activepaidseat.

Invariants: INV-037, INV-034.

## SC-S5-006 - Two cancel surfaces

**Given:** OTA andbackoffice race sameOrderdifferentkeys.

**When:** Dispatch cancellation.

**Then:** Onecanonicalhandler/claim; at mostonecommercialcancel.

Invariants: INV-003, INV-007, INV-034.

## SC-S6-001 - Capacity-free lounge

**Given:** Ancillaryeligiblelounge, AirPricequote30.

**When:** Accept Add.

**Then:** One typedservice/newitem/priceline30; FlightFlownevercalled.

Invariants: INV-035.

## SC-S6-002 - Capacity-bearing seat

**Given:** ExplicitseatproductrequiresFlightFlow.

**When:** Prepare/Add/Reserve.

**Then:** Soldseatproductseparatefromoperationalseat; capacityprotocolusedonlyrequiredscope.

Invariants: INV-035, INV-037.

## SC-S6-003 - Shared hotel

**Given:** One room-stay2guests3nights,total300.

**When:** Accept product.

**Then:** Oneindependentlycancellable room-stayservice,total300not600/900.

Invariants: INV-036, INV-012.

## SC-S6-004 - Shared transfer

**Given:** Onevehicle4beneficiaries,total80.

**When:** Accept product.

**Then:** Onevehicleservice withallbeneficiaries; total80.

Invariants: INV-036.

## SC-S6-005 - Through baggage

**Given:** Bagcovers2airservices; pieces1 maxweight23kg.

**When:** Accept andread.

**Then:** Onebagservice with2coverageportions andunits; no2ndbagcharge.

Invariants: INV-035, INV-036.

## SC-S6-006 - Included benefit

**Given:** Bundleincludesmeal withnoindependentprice.

**When:** Accept Order/Add profile.

**Then:** Trackserviceifindependentdeliveryneeded; nofabricatedzerocharge.

Invariants: INV-035.

## SC-S6-007 - Stale ancillary preparation

**Given:** BaseCV1; unrelatedcommercialchangeCV2.

**When:** Accept preparedquote.

**Then:** 409stale; no silentrebase/reprice.

Invariants: INV-054.

## SC-S6-008 - Supplier response loss

**Given:** Hotel supplierreservesroom butreplylost.

**When:** Recover acquisition.

**Then:** Originalsupplierresource/effect retained; no FlightFlowcapacityfake.

Invariants: INV-019, INV-020, INV-035.

## SC-S7-001 - Paid bag EMD-A

**Given:** AirETKTcoupon plusfundedbag20.

**When:** IssueEMDA.

**Then:** CorrectEmdCoupon associatedTicketCouponId andservice; onenumber.

Invariants: INV-028, INV-038.

## SC-S7-002 - Fee-only EMD-S

**Given:** Acceptedchangefee25 andnoactualservice.

**When:** IssueEMDS Fee.

**Then:** PricingLineRefrequired; no artificialserviceorcapacitycall.

Invariants: INV-038.

## SC-S7-003 - Residual EMD-S

**Given:** JetPayexternal residualvalue30.

**When:** IssueEMDS ResidualValue.

**Then:** Canonicalexternalvalueref; no newwalletbalance/capture.

Invariants: INV-026, INV-038.

## SC-S7-004 - EMD association change

**Given:** Associatedaircouponreissued.

**When:** Applyapprovedassociationplan.

**Then:** Appendold/newassociationhistory; previousissuedvalueunchanged.

Invariants: INV-038, INV-042.

## SC-S7-005 - Stale funding for paid add

**Given:** Originalcoverage120; newbag20notcovered.

**When:** IssuebagEMD.

**Then:** Blockuntilnewobligationfunded; nooldcoverage reuse.

Invariants: INV-025, INV-028, INV-038.

## SC-S8-001 - Document-only Void

**Given:** Eligibleunusedlocalticket, soldtotal120.

**When:** VoidmodeDocumentOnly.

**Then:** DocVoid andevent; CVandcommercialtotalunchanged; no automaticpayout.

Invariants: INV-039.

## SC-S8-002 - VoidAndCancel

**Given:** Approvedvoid/cancelplan withfundingdisposition.

**When:** Executecompoundusecase.

**Then:** Canonicalvoid+singlecommercialpivot, nosecondcancelrail.

Invariants: INV-034, INV-039, INV-054.

## SC-S8-003 - Used/control denied

**Given:** CouponUsedorexternallycontrolledwithoutrelease.

**When:** RequestVoid.

**Then:** Noirreversiblevoiddispatch.

Invariants: INV-039, INV-048.

## SC-S8-004 - Unknown external Void

**Given:** Externalissuerperformedvoid, replylost.

**When:** Retrykey.

**Then:** Suspend/recoveractualdoc; notmarkusableorallocatenewnumber.

Invariants: INV-020, INV-039.

## SC-S8-005 - Expired void window

**Given:** Ownerprofiledeadlinepassed.

**When:** Void.

**Then:** Knownineligibledenial; notgenericcalendar-dayguess.

Invariants: INV-039, INV-055.

## SC-S9-001 - Partial refund quote

**Given:** Sale120, AirPriceauthorizescredit50 andpenalty10.

**When:** Accept refund and acquire payoutauthority40.

**Then:** Commercialpivotnetcredit40, payout40, original120immutable.

Invariants: INV-040, INV-041.

## SC-S9-002 - Refund not old allocation

**Given:** Oldserviceallocation60; ownerrefunddecision45.

**When:** Refund.

**Then:** Use45decisionwithcaps, not60allocation.

Invariants: INV-014, INV-040.

## SC-S9-003 - Double reversal race

**Given:** Originaltax20; previousdirectreversal15.

**When:** Concurrentlyrequesttwoadditionalreversals5.

**Then:** At mostoneadditional5; directreversedtotal<=20.

Invariants: INV-040.

## SC-S9-004 - Payout response loss after pivot

**Given:** Commercialcredit40committed;ownerpayout40replylost.

**When:** Resume.

**Then:** CVdoesnotincrementagain; no2ndcredit or2ndrefund.

Invariants: INV-020, INV-041.

## SC-S9-005 - Payout permanently rejected after pivot

**Given:** Document/creditpivotcommitted, payoutfails.

**When:** Reconcile.

**Then:** Retainoutstandingpayable/NeedsReconciliation; do notreactivatecoupon.

Invariants: INV-041, INV-043.

## SC-S9-006 - Refund authority unknown before pivot

**Given:** Ownerreservesrefundfunds butreplylost.

**When:** Attemptpivot.

**Then:** Suspenduntilauthorityproof; no secondreservation.

Invariants: INV-020, INV-040.

## SC-S9-007 - Zero-cash compensation

**Given:** Acceptedcreditbutnooriginalcashrequires externalvaluedisposition.

**When:** Executeapprovedprofile.

**Then:** No inventedcardrefund; explicitownerconfirmedvalue/refundstate.

Invariants: INV-026, INV-040.

## SC-S9-008 - Goodwill beyond fare

**Given:** Originalvalue100, approvedgoodwill120.

**When:** Buildcredits.

**Then:** 100reversalplus20authorizedAdjustment, not120overreversal.

Invariants: INV-013, INV-040.

## SC-S9-009 - Historical FX

**Given:** Originaltaxconvertedusinghistoricalsourcevalue.

**When:** ReverseitafterFXrateschange.

**Then:** Copyhistoricalvaluation; no currentFXfetch/recalculation.

Invariants: INV-012, INV-040.

## SC-S10-001 - Two-to-one flight exchange

**Given:** Two old services replaced byone newtransportservice.

**When:** Accept fullsourcequoteandexecute.

**Then:** Many-to-onelineage; newstableID; oldservices/pricesretained.

Invariants: INV-042.

## SC-S10-002 - Price-only change

**Given:** Sameflightservice/scope; newacceptedcommercialprice.

**When:** Applyreplacementitem.

**Then:** KeepServiceId; appendnewmembership/pricing; nosecondseat.

Invariants: INV-009, INV-042.

## SC-S10-003 - Full values vs delta

**Given:** Old120,new150,ownerdecisiondelta30.

**When:** Normalizeandcommit.

**Then:** Exactlyoneapprovedtreatmentnet+30; not+150and+30.

Invariants: INV-012, INV-042.

## SC-S10-004 - New capacity fails

**Given:** Oldticketusable; newholddefinitivelyrejected.

**When:** Runexchange.

**Then:** Noolddocumentpivot; compensateonlyknownnewresources.

Invariants: INV-022, INV-043.

## SC-S10-005 - Unknown before externalissuerpivot

**Given:** Capacitynewcommitted;issuerUnknown.

**When:** Retryexchange.

**Then:** Sameoperation/documentroles; do notcancelnewcapacityuntilissueroutcomeknown.

Invariants: INV-020, INV-032, INV-043.

## SC-S10-006 - Cleanup Unknown after localpivot

**Given:** Newticket/pricepivotcommitted;oldcapacityreleaseUnknown.

**When:** Resume.

**Then:** Newcommercialtruthretained; recoveroldreleaseforward; no reactivationoldticket.

Invariants: INV-043.

## SC-S10-007 - Dependent paid seat replacement

**Given:** OldairhasseatEMD;newflightdiffers.

**When:** Executeexchange.

**Then:** ExplicitKeep/Replace/Cancel/ManualReview foreachdependency, associationhistorypreserved.

Invariants: INV-037, INV-042.

## SC-S10-008 - Residual refund

**Given:** Oldvalue150,new120,refundauthority30.

**When:** Completeexchange.

**Then:** Value30handledoncebyJetPay; completioneventsnotsecondcredit.

Invariants: INV-026, INV-041, INV-042.

## SC-S10-009 - Pricing coupling larger than selected leg

**Given:** One trueRTPUcouplesoutboundandreturn.

**When:** Quotechangeonlyoutbound.

**Then:** OwneraffectedPUscopereturnedandexplicitlyaccepted; unchangedserviceIDsretainedasappropriate.

Invariants: INV-015, INV-042.

## SC-S10-010 - Late delivery beforepivot

**Given:** Acceptedexchangequote thenflownobservationarrives.

**When:** Attemptpivot.

**Then:** Reevaluatevector/control; captureobservation; no blindstalepivot.

Invariants: INV-048, INV-043.

## SC-S11-001 - Impact is not remedy

**Given:** Disruptioncase affectsflight.

**When:** Recordimpact.

**Then:** Localimpactviewonly; CV/pricing/solditineraryunchanged.

Invariants: INV-044.

## SC-S11-002 - Independent 100Orderfanout

**Given:** 100affectedOrders;oneclaimblocked.

**When:** Processcaseinstruction.

**Then:** 99mayprogress;onepending; no100OrderTX.

Invariants: INV-045.

## SC-S11-003 - Duplicate case instruction

**Given:** Samecase/version/option/order repeated.

**When:** Redeliver.

**Then:** Sameoperation;noadditionalcollection/reissue.

Invariants: INV-007, INV-045.

## SC-S11-004 - Customer option expires

**Given:** Disruptionownerrequiresacceptancebutdeadlinebehaviorunknown.

**When:** Localclockpassesdeadline.

**Then:** No auto-reprice/refund/rebook; flagownerdecisionneeded.

Invariants: INV-044, INV-055.

## SC-S11-005 - Accepted involuntary waiver

**Given:** Ownercasewaiver+AirPriceapprovedquote.

**When:** Acceptremedy.

**Then:** Reuseexchangehandlerwithsourceauthority; pricingwaivernotinvented.

Invariants: INV-042, INV-044.

## SC-S12-001 - Batched source observations

**Given:** Oneeventcontains2passengerfacts.

**When:** Ingestthenrepeat.

**Then:** 2observationsbyobservationKey;no1lostor4duplicates.

Invariants: INV-046.

## SC-S12-002 - Board then offload

**Given:** CertifiedBoardingaspectBoardedrevision10 thenOffloaded11.

**When:** Ingest.

**Then:** BoardingcurrentOffloaded; noCouponUsed inferred.

Invariants: INV-046, INV-047.

## SC-S12-003 - Delayed older message

**Given:** Boardingrevision11alreadyapplied;revision9arrives.

**When:** Ingest.

**Then:** Historyretained;currentdoesnotregress.

Invariants: INV-046.

## SC-S12-004 - Unknown service correlation

**Given:** Externalfactrefunknown.

**When:** Ingest.

**Then:** Quarantinependingcorrelation; do notguessnearestflight/traveler.

Invariants: INV-046.

## SC-S12-005 - Schedule change

**Given:** Operationaldeparturenewtime.

**When:** Applyflightfact.

**Then:** SoldScheduleunchanged; currentoperationalviewupdated;CVunchanged.

Invariants: INV-047, INV-054.

## SC-S12-006 - Observation during claim

**Given:** Refund/exchangeclaimactive.

**When:** ReceiveauthoritativeFlown.

**Then:** Accepttruth; markconflictreconciliation; do notdropfact.

Invariants: INV-048.

## SC-S12-007 - Correct consumption

**Given:** Deliveredportion1latercorrectedbyauthoritativehigherrevision.

**When:** Applycorrection.

**Then:** Appendcorrectionlinkedtooriginal; onecorrectedeconomicdeliveryfact.

Invariants: INV-046, INV-047.

## SC-S12-008 - Partial bag consumption

**Given:** Bagcovers2portionsandoneconsumed.

**When:** Projectdelivery.

**Then:** Oneportionconsumed; otherunconsumed, noentirebagconsumption.

Invariants: INV-036, INV-046.

## SC-S12-009 - PII protection

**Given:** Protectedpassengerpayloadeligibleforerasure.

**When:** Eraseperapprovedpolicy.

**Then:** Personalpayloadinaccessible; nonPIImonetarydocumentlineageintact; events/logsminimized.

Invariants: INV-060.

## SC-S13-001 - Traveler name correction

**Given:** TravelerAhasissuedticket.

**When:** Acceptauthorizedprotectedcorrection.

**Then:** SameTravelerId; explicitcommercialchangeonce; doccorrection/revalidationseparateprofile.

Invariants: INV-049, INV-054, INV-060.

## SC-S13-002 - Guardian violation

**Given:** Lapinfantlinkedtoadult.

**When:** Remove/movesoleguardian.

**Then:** Rejectwithoutauthorityreplacement; noorphaninfant.

Invariants: INV-049, INV-011.

## SC-S13-003 - OpenAir no fake flight

**Given:** UndatedOpenAirservice.

**When:** Create/issueundersupportedopenprofile.

**Then:** No inventedFlightId/time/capacity;notdeliveryreadyuntilassigned.

Invariants: INV-049, INV-029.

## SC-S13-004 - Assign open flight

**Given:** OpenAirservicewithopencoupon;approvednewflightbinding.

**When:** Reserve/fundasrequired andacceptbinding.

**Then:** CommercialbindingchangeCV+1;historicalopenissuedsnapshotretained;documentassociationversioned.

Invariants: INV-049, INV-054.

## SC-S13-005 - Surface gap

**Given:** JourneycontainsSurfacebetweenairsegments.

**When:** Issueeligibleairservices.

**Then:** Noairseat/couponforthesurfacegap.

Invariants: INV-010, INV-049.

## SC-S13-006 - Unsupported name-change policy

**Given:** Requestreplacespassengeridentityratherthanpermittedcorrection.

**When:** Submitcorrection.

**Then:** ExplicitUnsupported/NeedsOwnerPolicy; notsilentidentityswap.

Invariants: INV-049, INV-058.

## SC-S14-001 - Whole traveler split

**Given:** Twoindependenttravelers;allownertransferproofavailable.

**When:** SplittravelerB.

**Then:** Service/TravelerIDsretained;childCV1/source+1;pairedmoneyconserved.

Invariants: INV-050.

## SC-S14-002 - Shared hotel acrosssplit

**Given:** RoomhasbeneficiariesA/B;onlyBselected.

**When:** Splitwithoutownerpartition.

**Then:** SharedServiceCannotBePartitioned; nosecondroomcopy.

Invariants: INV-036, INV-051.

## SC-S14-003 - Partial travel split

**Given:** TravelerBhasusedoutbound/openreturn.

**When:** Split.

**Then:** UsedcouponstaysUsed;OriginalOrderIdretained;currentservicingownerchanges.

Invariants: INV-050, INV-047.

## SC-S14-004 - Funding transfer Unknown

**Given:** Ownerapplicationsplitreserved butreplylost.

**When:** ResumeSplit.

**Then:** No duplicatedapplication;localownershippivotwaitsrequiredproof.

Invariants: INV-025, INV-050.

## SC-S14-005 - TwoOrdertransactionfailure

**Given:** SourceownershipchangeappliedinsideTX;childprojectionfails.

**When:** Commit.

**Then:** RollbackbothOrders/moneylinks;samestablechildIDreservedforretry.

Invariants: INV-003, INV-050.

## SC-S14-006 - Related links not merge

**Given:** TwoOrderslinkedforvisibility.

**When:** LinkRelatedOrders.

**Then:** No money/capacity/doccopy; originalsremainseparate.

Invariants: INV-051.

## SC-S14-007 - Different customer split

**Given:** SourcecustomerA,targetcustomerB.

**When:** RequestbaselineSplit.

**Then:** Unsupported; requireseparatepricedliabilitytransferdesign.

Invariants: INV-051, INV-058.

## SC-S15-001 - Per-block group cap

**Given:** Outbound10,inbound5confirmed;6namesrequireboth.

**When:** Materialize.

**Then:** At most5completebothflightallocations; sum15isnot15passengercapacity.

Invariants: INV-052.

## SC-S15-002 - Group uses existing capacity

**Given:** Groupblockseatreserved.

**When:** Materializename.

**Then:** Allocateexistingblock; nosecondgeneralsaleseat.

Invariants: INV-052.

## SC-S15-003 - Bulk49valid1invalid

**Given:** 50stableclientrowrefs withoneinvalidpassport.

**When:** Materializeandrepeatbatch.

**Then:** 49resultsstable;invalidrowreported;retrydoesnotrecharge/reissue49.

Invariants: INV-053.

## SC-S15-004 - Deposit transfer

**Given:** Deposit500with10childreneligibleshare50.

**When:** Applychildren.

**Then:** Externalbalancedapplicationssum<=500; nocopy500toeach.

Invariants: INV-026, INV-053.

## SC-S15-005 - Concurrent name allocation

**Given:** Twoworkersmaterializelastcapacityslot.

**When:** Race.

**Then:** Oneallocation; SQL/ownerresourceversionpreservescap.

Invariants: INV-052.

## SC-S15-006 - Charter duplicate charge

**Given:** Groupcontractalreadypriced.

**When:** Materializeexistingnamedslot.

**Then:** No newchartercontractcharge; explicitretailvalueonlyifapprovedpricing.

Invariants: INV-053.

## SC-S16-001 - Production simulation guard

**Given:** DeploymentprofileProductionplusSimulatorfunding.

**When:** Start.

**Then:** Failclosedbeforetraffic; noliveclaimfrommocktests.

Invariants: INV-057.

## SC-S16-002 - Complete regression

**Given:** Allimplementedstagesandmigrations.

**When:** Runallrequiredscenariosfromcleancheckout.

**Then:** Nonemptyresultsforeachrequiredscenario, noexcludedSQL/crashtests.

Invariants: INV-057.

## SC-S16-003 - Backuprestore unresolvedissue

**Given:** PersistedUnknownexternalissueandoutbox.

**When:** Restoretestbackupandrecover.

**Then:** Samekeys/numbers/read-back; no destructive replay.

Invariants: INV-019, INV-020, INV-031, INV-057.

## SC-S16-004 - Unsupportedfutureproduct

**Given:** Newschemahasnostatedowner/invariants.

**When:** Tryenable.

**Then:** Capabilitydisableduntilcontract/schema/testsregistered.

Invariants: INV-058.

## SC-S16-005 - SLO approval missing

**Given:** Benchmarksmeasuredbutnoapprovedtargets.

**When:** Assessrelease.

**Then:** Reportmeasurements;RELEASE_CERTIFIEDblockedBD013, notinventedSLAs.

Invariants: INV-057.

## SC-S16-006 - Ownercontractsemanticdrift

**Given:** Newadaptermappinglosesacceptedpricingfield.

**When:** Runcontract/regressioncompatibilitytests.

**Then:** Failcertification; noDomainredesigntohidemissingownersemantics.

Invariants: INV-058.

## SC-S14-008 - Refund ceilings after split

**Given:** Original value120 is partitioned into retained70 and transferred50.

**When:** Refund source and child under accepted owner decisions.

**Then:** Cap original-value reversal at70 in source and50 in child; combined lineage cannot refund more than120, and tender caps remain separate.

Invariants: INV-040, INV-050.
