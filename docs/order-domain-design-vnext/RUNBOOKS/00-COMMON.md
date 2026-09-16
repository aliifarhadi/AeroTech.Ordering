# Local execution contract for every stage

All commands run in the developer/agent terminal at the target repository root, not in Rancher and not on a production database. The Design Pack is documentation and test specifications; it does not claim the future .NET service or required script already exists.

B0 must physically deliver `scripts/Start-Local.ps1` and `scripts/Test-Stage.ps1` with these stable interfaces:

```powershell
Set-Location <path-to-AeroTech.Ordering>
./scripts/Start-Local.ps1 -Profile Reference
./scripts/Test-Stage.ps1 -Stage S1 -Profile Reference -EvidenceDirectory ./artifacts/stages/S1
```

The start script checks SDK/config, brings up disposable local SQL/broker/simulator services, applies safe development migrations, starts the host and emits its actual bound base URL. It must not use a hardcoded production endpoint, guess missing credentials, disable TLS validation or drop an existing database. Its configuration contract includes `Reference`, `LiveCandidateSandbox` and separately certified live profiles. A profile name alone never certifies an owner.

The test script maps Stage to required scenario IDs, runs nonempty test selections, collects TRX/structured evidence, validates expected effect counts and produces nonzero exit on failure, skip of a required test, missing host/SQL, or missing required scenario. Unknown live owner access is reported separately and cannot downgrade mandatory reference tests. Supplying `-Stage All` executes every implemented prior stage without silently excluding persistence/recovery.

The Agent must create per-stage `API/requests.http` and `E2E/input.json` from the exact working API contract. Fields named preparation/order/service/document IDs in the runbook are populated from preceding responses, not copied static fake IDs. Run `GetOrder` after each actual mutation; compare source commercial version, current revision and independent owner facets.

To demonstrate owner-outage reads, disable only the relevant outbound provider connectivity or stop a disposable provider process, keep Ordering SQL available, restart Ordering and repeat GetOrder. Record the actual action. Do not stop production owners. For process crash tests record which process stopped and at which named durable boundary. Redact credentials/PII in evidence.
