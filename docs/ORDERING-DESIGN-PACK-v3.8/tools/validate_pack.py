#!/usr/bin/env python3
"""Validate the Design Pack itself; this does not validate the future .NET implementation."""
from __future__ import annotations
import argparse
from decimal import Decimal
import hashlib
import json
from pathlib import Path
import re
import sys


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument('root', nargs='?', default=str(Path(__file__).resolve().parents[1]))
    parser.add_argument('--checksums', action='store_true')
    parser.add_argument('--output')
    args = parser.parse_args()
    root = Path(args.root).resolve()
    checks: list[dict] = []

    def check(name: str, ok: bool, detail: str = '') -> None:
        checks.append({'check': name, 'result': 'PASS' if ok else 'FAIL', 'detail': detail})

    def load(path: str):
        return json.loads((root / path).read_text(encoding='utf-8'))

    def text(path: str) -> str:
        return (root / path).read_text(encoding='utf-8')

    try:
        import jsonschema
    except ImportError:
        print('Missing jsonschema dependency; see requirements-validation.txt. No schema certification was performed.', file=sys.stderr)
        return 2

    required = [
        'README.md','START-HERE.md','AGENT-START-PROMPT.md','IMPLEMENTATION-INSTRUCTIONS.md','MANIFEST.md','VALIDATION.md',
        'BLOCKED-DECISIONS.md','VERTICAL-SLICE-PLAN.md','SCENARIO-CATALOG.md','TESTING-AND-CERTIFICATION.md',
        'API-CONTRACTS.md','EVENT-CATALOG.md','GOVERNANCE/05-STANDING-RULE-CONFLICT-RESOLUTION.md',
        'REVIEW/01-PREVIOUS-PACK-REVIEW.md','REVIEW/02-TARGET-BASELINE.md','REVIEW/03-SOURCE-REGISTER.md',
        'REVIEW/05-PACK-2.1-SCOPE-CORRECTION.md','CONTRACTS/12-REFERENCE-DATA-AND-CUSTOMER.md',
        'SPEC/openapi-s1.json','SPEC/layers.json','SPEC/negative-fixtures.json','EXAMPLES/pricing-ledger.json'
    ]
    missing = [p for p in required if not (root / p).is_file()]
    check('Required entrypoints and specification files', not missing, ', '.join(missing))

    datasets = {n: load('SPEC/' + n + '.json') for n in ['stages','scenarios','invariants','commands','events','interactions','ports']}
    for n, rows in datasets.items():
        field = 'name' if n == 'ports' else 'id'
        ids = [r[field] for r in rows]
        check('Unique ' + n + ' identities', len(ids) == len(set(ids)), str(len(ids)) + ' entries')

    stages = {s['id']: s for s in datasets['stages']}
    invs = {s['id']: s for s in datasets['invariants']}
    cases = {s['id']: s for s in datasets['scenarios']}
    commands = {s['id']: s for s in datasets['commands']}
    expect = ['D0','B0'] + ['S' + str(i) for i in range(1,17)]
    check('Exact stage sequence', list(stages) == expect)

    for sid, stage in stages.items():
        errors = []
        for field in ['sliceFile','runbook','primarySpecification']:
            if not (root / stage[field]).is_file(): errors.append('missing ' + stage[field])
        if not stage['scenarios']: errors.append('no scenarios')
        for case in stage['scenarios']:
            if case not in cases or cases[case]['stage'] != sid: errors.append('invalid scenario ' + case)
        for command in stage['commands']:
            if command not in commands or commands[command]['stage'] != sid: errors.append('invalid command ' + command)
        if any(p not in expect[:expect.index(sid)] for p in stage['prerequisites']): errors.append('invalid dependency')
        content = text(stage['sliceFile'])
        for heading in ['Goal and user-observable result','Physical layer changes','Persistence, transaction','Idempotency, concurrency','Acceptance tests','Manual E2E runbook','DONE gate','Artifacts produced at end','Explicitly not in this slice']:
            if heading not in content: errors.append('missing ' + heading)
        check(sid + ' stage contract', not errors, '; '.join(errors))

    used = set(); errors = []
    for scenario in datasets['scenarios']:
        if scenario['stage'] not in stages: errors.append(scenario['id'] + ' stage')
        elif scenario['id'] not in stages[scenario['stage']]['scenarios']: errors.append(scenario['id'] + ' missing in stage index')
        for inv in scenario['invariants']:
            used.add(inv)
            if inv not in invs: errors.append(scenario['id'] + ' invalid ' + inv)
        if scenario['runtimeEvidence'] != 'NOT_EXECUTED_IN_DESIGN_DELIVERY': errors.append(scenario['id'] + ' fabricated execution')
    check('Scenario references and no fabricated runtime evidence', not errors, '; '.join(errors))
    check('Every invariant has scenario coverage', set(invs) <= used, ', '.join(sorted(set(invs) - used)))
    check('Invariant detailed documents exist', all((root / i['source']).is_file() for i in invs.values()))

    fields = ['caller','owner','endpointSurface','semanticType','interaction','transport','transactionOwner','idempotency','timeoutSemantics','retrySemantics','unknownSemantics','authoritativeRecovery','ttlExpiryOwner','readModelEffect']
    errors = [x['id'] + ':' + f for x in datasets['interactions'] for f in fields if not x.get(f)]
    check('All fourteen interaction dimensions', not errors, ', '.join(errors))
    handlers = [c['canonicalHandler'] for c in commands.values()]
    check('One canonical handler identity per command/query', len(handlers) == len(set(handlers)))
    check('No business admin surface confusion', all(c['surfaces'] == ['internal'] if c['id'].startswith('ADM') else 'internal' not in c['surfaces'] for c in commands.values()))

    # Every machine requirement is indexed by exactly its declared stage and visible in that stage's slice.
    trace_errors = []
    for c in datasets['commands']:
        if c['stage'] not in stages or c['id'] not in stages[c['stage']]['commands']: trace_errors.append('command:' + c['id'])
    for e in datasets['events']:
        if e['stage'] not in stages or e['name'] not in stages[e['stage']]['events']: trace_errors.append('event:' + e['id'])
    for i in datasets['invariants']:
        if i['introducedBy'] not in stages or i['id'] not in stages[i['introducedBy']]['introducedInvariants']: trace_errors.append('invariant:' + i['id'])
    for sid, stage in stages.items():
        slice_text = text(stage['sliceFile'])
        for kind, ids in [('scenario',stage['scenarios']),('invariant',stage['introducedInvariants']),('command',stage['commands']),('event',stage['events'])]:
            for item in ids:
                if item not in slice_text: trace_errors.append(f'{sid}:{kind}:{item}')
    check('Stage indexes and slices trace every command/event/invariant/scenario', not trace_errors, '; '.join(trace_errors))

    # Markdown links.
    errors = []
    for p in root.rglob('*.md'):
        for link in re.findall(r'\[[^\]]*\]\(([^)]+)\)', p.read_text(encoding='utf-8')):
            if re.match(r'^[a-zA-Z]+:', link) or link.startswith('#'): continue
            rel = link.split('#')[0]
            if not (p.parent / rel).exists(): errors.append(str(p.relative_to(root)) + ':' + rel)
    check('Relative Markdown file links', not errors, '; '.join(errors))

    # Exact pack-owned paths written as code spans must resolve. This catches orphaned EXAMPLES/SPEC refs.
    prefixes = ('ARCHITECTURE/','CONTRACTS/','DOMAIN/','EXAMPLES/','GOVERNANCE/','REVIEW/','RUNBOOKS/','SLICES/','SPEC/','tools/')
    root_names = {'README.md','START-HERE.md','AGENT-START-PROMPT.md','IMPLEMENTATION-INSTRUCTIONS.md','MANIFEST.md','VALIDATION.md','BLOCKED-DECISIONS.md','VERTICAL-SLICE-PLAN.md','SCENARIO-CATALOG.md','TESTING-AND-CERTIFICATION.md','API-CONTRACTS.md','EVENT-CATALOG.md','LEGACY-CUTOVER-OUT-OF-SCOPE.md'}
    missing_refs = []
    for p in root.rglob('*.md'):
        for val in re.findall(r'`([^`\n]+)`', p.read_text(encoding='utf-8')):
            val = val.strip().rstrip('.,;:').split('#')[0]
            if any(ch in val for ch in '*{}<>'): continue
            if val.startswith(prefixes) or val in root_names:
                if Path(val).suffix.lower() in {'.md','.json','.py','.txt'} or val in root_names:
                    if not (root / val).exists(): missing_refs.append(str(p.relative_to(root)) + ':' + val)
    check('Exact pack-owned code-span references resolve', not missing_refs, '; '.join(missing_refs))

    # MANIFEST inventory (SHA256SUMS is generated separately).
    manifest_list = set(re.findall(r'^- `([^`]+)`$', text('MANIFEST.md'), re.M))
    actual_manifest = {str(p.relative_to(root)) for p in root.rglob('*') if p.is_file() and p.name != 'SHA256SUMS' and '__pycache__' not in p.parts}
    check('MANIFEST inventory complete and exact', manifest_list == actual_manifest, ', '.join(sorted(manifest_list ^ actual_manifest)))

    # Machine scenario catalog must be exactly mirrored by the human catalog.
    catalog_text = text('SCENARIO-CATALOG.md')
    blocks = {}
    for m in re.finditer(r'^## (SC-[A-Z0-9-]+) - (.+)$', catalog_text, re.M):
        nxt = re.search(r'^## SC-', catalog_text[m.end():], re.M)
        end = len(catalog_text) if not nxt else m.end() + nxt.start()
        blocks[m.group(1)] = (m.group(2).strip(), catalog_text[m.start():end])
    def norm(v: str | None) -> str | None:
        if v is None: return None
        return v.replace('`','').rstrip('.').strip()
    errors = []
    for scenario in datasets['scenarios']:
        block = blocks.get(scenario['id'])
        if not block:
            errors.append(scenario['id'] + ':missing'); continue
        title, body = block
        if title != scenario['title']: errors.append(scenario['id'] + ':title')
        for label, key in [('Given','given'),('When','when'),('Then','then')]:
            m = re.search(r'\*\*' + label + r':\*\* (.+)', body)
            if norm(m.group(1) if m else None) != norm(scenario[key]): errors.append(scenario['id'] + ':' + key)
    extras = set(blocks) - set(cases)
    errors.extend('extra:' + x for x in sorted(extras))
    check('Scenario catalog mirrors SPEC/scenarios', not errors, '; '.join(errors))

    # Event catalog must mirror event machine spec and use role-correct economic rules.
    event_text = text('EVENT-CATALOG.md'); errors = []
    role_phrase = {
        'CommercialMovement':'single committed PriceChangeSet for this commercial change',
        'DocumentEvidence':'does not create another commercial PriceChangeSet',
        'PaymentEvidence':'never create a second commercial credit or PriceChangeSet',
        'DeliveryEvidence':'does not create a commercial PriceChangeSet',
        'Reclassification':'does not create new customer value',
        'WorkflowCompletion':'does not create a second commercial PriceChangeSet'
    }
    for event in datasets['events']:
        m = re.search(r'^## ' + re.escape(event['id']) + r' - ' + re.escape(event['name']) + r'\n(.*?)(?=^## EV-|\Z)', event_text, re.M | re.S)
        if not m:
            errors.append(event['id'] + ':missing'); continue
        body = m.group(1)
        if f"Stage: {event['stage']}. EconomicRole: `{event['role']}`." not in body: errors.append(event['id'] + ':stage/role')
        pm = re.search(r'Payload: (.+)\.', body)
        payload = re.findall(r'`([^`]+)`', pm.group(1)) if pm else []
        if payload != event['requiredPayloadFields']: errors.append(event['id'] + ':payload')
        if event['economicInterpretation'] not in body: errors.append(event['id'] + ':economic')
        if event['rule'] not in body: errors.append(event['id'] + ':rule')
        if role_phrase[event['role']] not in event['rule']: errors.append(event['id'] + ':role-rule')
    check('Event catalog mirrors SPEC/events with role-correct economic rules', not errors, '; '.join(errors))

    # Source fingerprint IDs must be represented in the human source register.
    fingerprints = load('REVIEW/source-fingerprints.json')
    fp_ids = {x['id'] for x in fingerprints['repositorySources']}
    register = text('REVIEW/03-SOURCE-REGISTER.md')
    missing_source_ids = [i for i in sorted(fp_ids) if f'## {i}' not in register]
    check('Source fingerprints represented in source register', not missing_source_ids, ', '.join(missing_source_ids))

    # Schema fixtures.
    pairs = [('normalized-candidate.json','normalized-candidate.schema.json'),('create-request.json','create-request.schema.json'),('capacity-effect-confirmed.json','capacity-effect-result.schema.json'),('capacity-effect-unknown.json','capacity-effect-result.schema.json'),('stage-status-not-executed.json','stage-status.schema.json')]
    for data, sch in pairs:
        errors = []
        validator = jsonschema.Draft202012Validator(load('SPEC/schemas/' + sch), format_checker=jsonschema.FormatChecker())
        try: validator.validate(load('EXAMPLES/' + data))
        except jsonschema.ValidationError as e: errors.append(e.message)
        check('Schema fixture ' + data, not errors, '; '.join(errors))

    def candidate_error(c):
        travelers = [x['sourceTravellerRef'] for x in c['travelers']]
        services = [x['serviceRef'] for x in c['services']]
        members = [ref for i in c['items'] for ref in i['serviceRefs']]
        if len(travelers) != len(set(travelers)) or len(services) != len(set(services)): return 'DuplicateIdentity'
        if set(members) != set(services) or len(members) != len(set(members)): return 'ServiceMembershipMismatch'
        if any(b not in travelers for s in c['services'] for b in s['beneficiaryRefs']): return 'UnknownBeneficiary'
        if any(l['component'] == 'Tax' and l['effect'] == 'SettlementOnly' for l in c['pricingLines']): return 'TaxSettlementOnlyForbidden'
        lines = [l for l in c['pricingLines'] if l['effect'] == 'CustomerBalance']
        if any(l['saleValue']['currencyRef'] != c['customerTotal']['currencyRef'] for l in lines): return 'MixedCurrency'
        value = sum((Decimal(l['saleValue']['amount']) * (1 if l['direction'] == 'Debit' else -1) for l in lines), Decimal(0))
        if value != Decimal(c['customerTotal']['amount']): return 'CustomerTotalMismatch'
        return None

    check('Positive fixture identity/money consistency', candidate_error(load('EXAMPLES/normalized-candidate.json')) is None)
    negative_refs = load('SPEC/negative-fixtures.json')
    missing_negative = [n['file'] for n in negative_refs if not (root / n['file']).is_file()]
    wrong_negative = [n['file'] for n in negative_refs if not n['file'].startswith('EXAMPLES/negative/')]
    check('Negative fixture references exist', not missing_negative, ', '.join(missing_negative))
    check('Negative fixture references stay under EXAMPLES/negative', not wrong_negative, ', '.join(wrong_negative))
    for n in negative_refs:
        if not (root / n['file']).is_file(): continue
        x = load(n['file'])
        if n['rule'] == 'SchemaConfirmedEvidence':
            err = list(jsonschema.Draft202012Validator(load('SPEC/schemas/capacity-effect-result.schema.json')).iter_errors(x)); ok = bool(err)
        else:
            ok = candidate_error(x) == n['rule']
        check('Negative fixture ' + n['rule'], ok)

    ledger = load('EXAMPLES/pricing-ledger.json')
    def signed(lines):
        return sum((Decimal(x['amount']) * (1 if x['direction'] == 'Debit' else -1) for x in lines if x['effect'] == 'CustomerBalance'), Decimal(0))
    sale = signed(ledger['commercialLines'])
    after_rev = sale + signed(ledger['servicingCommercialLines'])
    after_refund = after_rev + signed(ledger['refundAuthorizationLines'])
    check('Pricing ledger sale total', sale == Decimal(ledger['expectedCustomerTotalAfterSale']))
    check('Pricing ledger discount-reversal total', after_rev == Decimal(ledger['expectedCustomerTotalAfterDiscountReversal']))
    check('Pricing ledger refund-authorization total', after_refund == Decimal(ledger['expectedCustomerTotalAfterRefundAuthorization']))
    check('Pricing ledger payment movement does not duplicate commercial credit', after_refund == Decimal(ledger['expectedCustomerTotalAfterPaymentMovement']))

    events = datasets['events']
    check('Event spec does not redefine shared envelope fields', all('envelopeFields' not in e for e in events))
    check('Event spec binds existing platform envelope', all(bool(e.get('envelopeBinding')) for e in events))

    layers = load('SPEC/layers.json')
    check('Layer spec is logical policy, not physical reference contract', layers.get('kind') == 'LogicalResponsibilityAndDependencyPolicy')
    allow = layers.get('standingConventions', {}).get('domainMessagesAllowlist', {})
    expected_exact = [
        'AeroTech.Messages.Aegis.Enums.BusinessContextType',
        'AeroTech.Messages.Aegis.Enums.PrincipalType',
        'AeroTech.Messages.Shared.Enums.AuthorizationSurface'
    ]
    check('Domain Messages namespace allowlist is exact', allow.get('namespacePrefixes') == ['AeroTech.Messages.Ordering.Enums'])
    check('Domain Messages non-Ordering type allowlist is exact', allow.get('exactTypes') == expected_exact)
    check('Domain Messages project reference may remain', allow.get('projectReferenceMayRemain') is True)
    check('New non-Ordering Domain Messages type requires user approval', allow.get('newNonOrderingMessageTypeRequiresExplicitUserApproval') is True)

    b0 = stages['B0']; b0case = cases['SC-B0-001']
    check('B0 stage does not demand Domain->Messages project-reference removal', all(not (w.lower().startswith('repair domain') and 'messages' in w.lower()) for w in b0['work']))
    check('SC-B0-001 is allowlist-based, not remove-reference-based', 'remove dependency' not in b0case['then'].lower() and 'project reference' in b0case['then'].lower())
    b0docs = text('SLICES/B0.md') + '\n' + text('RUNBOOKS/B0.md')
    check('B0 docs carry exact caller-context allowlist', all(x in b0docs for x in ['BusinessContextType','PrincipalType','AuthorizationSurface','IHomeOperatorProvider']))

    gov = text('GOVERNANCE/05-STANDING-RULE-CONFLICT-RESOLUTION.md')
    check('C15 keeps OwnerAirlineId on home-operator provider, not caller context', all(x in gov for x in ['IHomeOperatorProvider','ReferenceDataHomeOperatorProvider','ICallerContext']))

    replay = cases['SC-S1-003']['then']
    check('S1 replay does not require invented client-visible replay flag', 'with replay flag' not in replay.lower() and 'this pack invents no replay header/flag' in replay.lower())
    semantic_files = [p for p in root.rglob('*') if p.is_file() and p.suffix in {'.md','.json'}]
    all_text = '\n'.join(p.read_text(encoding='utf-8', errors='ignore') for p in semantic_files)
    check('No Idempotent-Replay custom header remains', ('Idempotent' + '-Replay') not in all_text)
    check('API replay contract explicitly avoids invented metadata shape', 'this pack does not invent a replay metadata shape' in text('API-CONTRACTS.md'))

    lower_all = all_text.lower()
    check('No stale assume-existing deterministic-provider instruction', not re.search(r'\buse\s+the\s+existing\s+deterministic\s+provider\b', lower_all) and not re.search(r'\bexisting\s+deterministic\s+provider\b', lower_all))
    check('Current pack has no stale 3.7 version reference', ('Pack 3.' + '7') not in all_text and ('v3.' + '7') not in all_text)
    check('Agent reads complete governance/review/contract ranges', all(x in text('AGENT-START-PROMPT.md') for x in ['GOVERNANCE/01..05','REVIEW/01..05','CONTRACTS/00..12']))
    check('Validation report does not hard-code validator pass count', re.search(r'\*\*\d+ checks passed, 0 failed\*\*', text('VALIDATION.md')) is None)

    # Human-readable requirement counts must match machine indexes.
    manifest_counts = text('MANIFEST.md')
    validation_counts = text('VALIDATION.md')
    count_expect = {
        'stages': len(datasets['stages']), 'invariants': len(datasets['invariants']), 'scenarios': len(datasets['scenarios']),
        'commands': len(datasets['commands']), 'ports': len(datasets['ports']), 'events': len(datasets['events']), 'interactions': len(datasets['interactions'])
    }
    manifest_ok = (f"{count_expect['stages']} stages" in manifest_counts and f"{count_expect['invariants']} invariants" in manifest_counts and f"{count_expect['scenarios']} acceptance scenarios" in manifest_counts and f"{count_expect['commands']} canonical command/query contracts" in manifest_counts and f"{count_expect['ports']} semantic ports" in manifest_counts and f"{count_expect['events']} event types" in manifest_counts and f"{count_expect['interactions']} expanded interactions" in manifest_counts)
    validation_ok = (f"- stages: {count_expect['stages']}" in validation_counts and f"- scenarios: {count_expect['scenarios']}" in validation_counts and f"- invariants: {count_expect['invariants']}" in validation_counts and f"- commands/queries: {count_expect['commands']}" in validation_counts and f"- semantic external ports: {count_expect['ports']}" in validation_counts and f"- integration event types: {count_expect['events']}" in validation_counts and f"- expanded service interactions: {count_expect['interactions']}" in validation_counts)
    check('Human requirement counts match machine indexes', manifest_ok and validation_ok)

    for v in load('SPEC/pricing-example-vectors.json'):
        total = sum(map(Decimal, v['customerDebits']), Decimal(0)) - sum(map(Decimal, v['customerCredits']), Decimal(0))
        check(v['id'] + ' decimal arithmetic', total == Decimal(v['expectedCustomerTotal']))

    oa = load('SPEC/openapi-s1.json'); missing_refs = []
    def visit(node):
        if isinstance(node, dict):
            if '$ref' in node:
                ref = node['$ref']
                if not ref.startswith('#/'): missing_refs.append(ref)
                else:
                    cur = oa
                    try:
                        for part in ref[2:].split('/'): cur = cur[part.replace('~1','/').replace('~0','~')]
                    except (KeyError, TypeError): missing_refs.append(ref)
            for val in node.values(): visit(val)
        elif isinstance(node, list):
            for val in node: visit(val)
    visit(oa)
    check('S1 OpenAPI internal references', not missing_refs, ', '.join(missing_refs))
    oaids = [op['operationId'] for p in oa['paths'].values() for op in p.values()]
    check('S1 OpenAPI sixteen distinct surface operations', len(oaids) == 16 and len(oaids) == len(set(oaids)))

    if args.checksums:
        sums = root / 'SHA256SUMS'; errors = []; listed = set()
        if not sums.is_file():
            errors.append('SHA256SUMS missing')
        else:
            for line in sums.read_text(encoding='utf-8').splitlines():
                digest, rel = line.split('  ', 1); listed.add(rel); p = root / rel
                if not p.is_file() or hashlib.sha256(p.read_bytes()).hexdigest() != digest: errors.append(rel)
            actual = {str(p.relative_to(root)) for p in root.rglob('*') if p.is_file() and p.name != 'SHA256SUMS' and '__pycache__' not in p.parts}
            check('SHA256 inventory complete', listed == actual, ', '.join(sorted(listed ^ actual)))
        check('SHA256 file integrity', not errors, ', '.join(errors))

    result = {
        'artifact':'Design specification only',
        'checks':checks,
        'passed':sum(x['result'] == 'PASS' for x in checks),
        'failed':sum(x['result'] == 'FAIL' for x in checks),
        'counts':{n:len(x) for n,x in datasets.items()},
        'dotnetBuild':'NOT_EXECUTED',
        'sqlRuntimeTests':'NOT_EXECUTED',
        'liveOwnerE2E':'NOT_EXECUTED',
        'productionCertification':False
    }
    encoded = json.dumps(result, indent=2)
    if args.output: Path(args.output).write_text(encoded + '\n', encoding='utf-8')
    print(encoded)
    return 1 if result['failed'] else 0

if __name__ == '__main__':
    raise SystemExit(main())
