# Common runbook contract — behavior, not developer environment

These runbooks specify **business/integration actions and observable assertions** for each stage. They intentionally do not prescribe where SQL Server runs, whether Docker is used, how RabbitMQ is provisioned, which JWT/test issuer exists, which shell/script launches the service, or where evidence files are stored. Those are user/team/repository execution mechanics.

For every stage, use the existing repository-approved mechanism to:

1. build/start the relevant Ordering host and required dependencies;
2. apply the stage's migrations to an approved test database/environment;
3. execute the exact scenario inputs described by the stage;
4. inspect local state and owner/simulator evidence only through approved test/diagnostic mechanisms;
5. restart/crash processes at the named durable boundaries when recovery semantics are under test;
6. record actual requests/results/effect counts and redact secrets/PII.

A runbook step that says “owner unavailable” means the test must make the relevant dependency unavailable using a team-approved test mechanism; it does not authorize stopping or mutating a shared/production owner. A step that says “SQL persistence” requires SQL Server semantics appropriate to the target system, but this pack does not choose a server instance or provisioning method.

The acceptance criterion is the observed behavior and preserved invariant, not a particular command line.
