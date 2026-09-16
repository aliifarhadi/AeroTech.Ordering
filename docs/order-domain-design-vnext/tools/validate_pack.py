#!/usr/bin/env python3
"""Validate a Design Pack, not the future Ordering implementation."""
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
    def load(name: str):
        return json.loads((root / name).read_text(encoding='utf-8'))
    try:
        import jsonschema
    except ImportError:
        print('Missing jsonschema dependency; see requirements-validation.txt. No schema certification was performed.', file=sys.stderr)
        return 2
    required = ['README.md','START-HERE.md','AGENT-START-PROMPT.md','IMPLEMENTATION-INSTRUCTIONS.md','MANIFEST.md','VALIDATION.md',
                'BLOCKED-DECISIONS.md','VERTICAL-SLICE-PLAN.md','SCENARIO-CATALOG.md','TESTING-AND-CERTIFICATION.md',
                'API-CONTRACTS.md','EVENT-CATALOG.md','REVIEW/01-PREVIOUS-PACK-REVIEW.md','REVIEW/02-TARGET-BASELINE.md',
                'REVIEW/03-SOURCE-REGISTER.md','SPEC/openapi-s1.json']
    missing = [p for p in required if not (root/p).is_file()]
    check('Required entrypoints and specification files', not missing, ', '.join(missing))
    datasets = {n: load('SPEC/'+n+'.json') for n in ['stages','scenarios','invariants','commands','events','interactions','ports']}
    for n,rows in datasets.items():
        field = 'name' if n == 'ports' else 'id'
        ids = [r[field] for r in rows]
        check('Unique '+n+' identities',len(ids)==len(set(ids)),str(len(ids))+' entries')
    stages = {s['id']:s for s in datasets['stages']}
    invs = {s['id']:s for s in datasets['invariants']}
    cases = {s['id']:s for s in datasets['scenarios']}
    cmd = {s['id']:s for s in datasets['commands']}
    expect = ['D0','B0']+['S'+str(i) for i in range(1,17)]
    check('Exact stage sequence', list(stages)==expect)
    for sid,s in stages.items():
        errors=[]
        for field in ['sliceFile','runbook','primarySpecification']:
            if not (root/s[field]).is_file(): errors.append('missing '+s[field])
        if not s['scenarios']: errors.append('no scenarios')
        for case in s['scenarios']:
            if case not in cases or cases[case]['stage']!=sid:errors.append('invalid scenario '+case)
        for c in s['commands']:
            if c not in cmd or cmd[c]['stage']!=sid:errors.append('invalid command '+c)
        prior=s['prerequisites']
        if any(p not in expect[:expect.index(sid)] for p in prior):errors.append('invalid dependency')
        content=(root/s['sliceFile']).read_text()
        for heading in ['Goal and user-observable result','Physical layer changes','Persistence, transaction','Idempotency, concurrency','Acceptance tests','Manual E2E runbook','DONE gate','Artifacts produced at end','Explicitly not in this slice']:
            if heading not in content:errors.append('missing '+heading)
        check(sid+' stage contract',not errors,'; '.join(errors))
    used=set()
    errors=[]
    for s in datasets['scenarios']:
        if s['stage'] not in stages:errors.append(s['id']+' stage')
        if s['id'] not in stages[s['stage']]['scenarios']:errors.append(s['id']+' missing in stage index')
        for inv in s['invariants']:
            used.add(inv)
            if inv not in invs:errors.append(s['id']+' invalid '+inv)
        if s['runtimeEvidence']!='NOT_EXECUTED_IN_DESIGN_DELIVERY':errors.append(s['id']+' fabricated execution')
    check('Scenario references and no fabricated runtime evidence',not errors,'; '.join(errors))
    check('Every invariant has scenario coverage',set(invs)<=used,', '.join(sorted(set(invs)-used)))
    check('Invariant detailed documents exist',all((root/i['source']).is_file() for i in invs.values()))
    fields=['caller','owner','endpointSurface','semanticType','interaction','transport','transactionOwner','idempotency','timeoutSemantics','retrySemantics','unknownSemantics','authoritativeRecovery','ttlExpiryOwner','readModelEffect']
    errors=[x['id']+':'+f for x in datasets['interactions'] for f in fields if not x.get(f)]
    check('All fourteen interaction dimensions',not errors,', '.join(errors))
    handlers=[c['canonicalHandler'] for c in cmd.values()]
    check('One canonical handler identity per command/query',len(handlers)==len(set(handlers)))
    check('No business admin surface confusion',all(c['surfaces']==['internal'] if c['id'].startswith('ADM') else 'internal' not in c['surfaces'] for c in cmd.values()))
    errors=[]
    for p in root.rglob('*.md'):
        for link in re.findall(r'\[[^\]]*\]\(([^)]+)\)',p.read_text()):
            if re.match(r'^[a-zA-Z]+:',link) or link.startswith('#'):continue
            rel=link.split('#')[0]
            if not (p.parent/rel).exists():errors.append(str(p.relative_to(root))+':'+rel)
    check('Relative Markdown file links',not errors,'; '.join(errors))
    pairs=[('normalized-candidate.json','normalized-candidate.schema.json'),('create-request.json','create-request.schema.json'),('capacity-effect-confirmed.json','capacity-effect-result.schema.json'),('capacity-effect-unknown.json','capacity-effect-result.schema.json'),('stage-status-not-executed.json','stage-status.schema.json')]
    for data,sch in pairs:
        errors=[]
        validator=jsonschema.Draft202012Validator(load('SPEC/schemas/'+sch),format_checker=jsonschema.FormatChecker())
        try:validator.validate(load('EXAMPLES/'+data))
        except jsonschema.ValidationError as e:errors.append(e.message)
        check('Schema fixture '+data,not errors,'; '.join(errors))
    def candidate_error(c):
        from decimal import Decimal
        travelers=[x['sourceTravellerRef'] for x in c['travelers']]
        services=[x['serviceRef'] for x in c['services']]
        members=[ref for i in c['items'] for ref in i['serviceRefs']]
        if len(travelers)!=len(set(travelers)) or len(services)!=len(set(services)):return 'DuplicateIdentity'
        if set(members)!=set(services) or len(members)!=len(set(members)):return 'ServiceMembershipMismatch'
        if any(b not in travelers for s in c['services'] for b in s['beneficiaryRefs']):return 'UnknownBeneficiary'
        if any(l['component']=='Tax' and l['effect']=='SettlementOnly' for l in c['pricingLines']):return 'TaxSettlementOnlyForbidden'
        lines=[l for l in c['pricingLines'] if l['effect']=='CustomerBalance']
        if any(l['saleValue']['currencyRef']!=c['customerTotal']['currencyRef'] for l in lines):return 'MixedCurrency'
        value=sum((Decimal(l['saleValue']['amount'])*(1 if l['direction']=='Debit' else -1) for l in lines),Decimal(0))
        if value!=Decimal(c['customerTotal']['amount']):return 'CustomerTotalMismatch'
        return None
    check('Positive fixture identity/money consistency',candidate_error(load('EXAMPLES/normalized-candidate.json')) is None)
    for n in load('SPEC/negative-fixtures.json'):
        x=load(n['file'])
        if n['rule']=='SchemaConfirmedEvidence':
            err=list(jsonschema.Draft202012Validator(load('SPEC/schemas/capacity-effect-result.schema.json')).iter_errors(x))
            ok=bool(err)
        else:ok=candidate_error(x)==n['rule']
        check('Negative fixture '+n['rule'],ok)
    for v in load('SPEC/pricing-example-vectors.json'):
        total=sum(map(Decimal,v['customerDebits']),Decimal(0))-sum(map(Decimal,v['customerCredits']),Decimal(0))
        check(v['id']+' decimal arithmetic',total==Decimal(v['expectedCustomerTotal']))
    oa=load('SPEC/openapi-s1.json')
    missing=[]
    def visit(node):
        if isinstance(node,dict):
            if '$ref' in node:
                ref=node['$ref']
                if not ref.startswith('#/'):missing.append(ref)
                else:
                    cur=oa
                    try:
                        for part in ref[2:].split('/'):cur=cur[part.replace('~1','/').replace('~0','~')]
                    except (KeyError,TypeError):missing.append(ref)
            for val in node.values():visit(val)
        elif isinstance(node,list):
            for val in node:visit(val)
    visit(oa)
    check('S1 OpenAPI internal references',not missing,', '.join(missing))
    oaids=[op['operationId'] for p in oa['paths'].values() for op in p.values()]
    check('S1 OpenAPI sixteen distinct surface operations',len(oaids)==16 and len(oaids)==len(set(oaids)))
    if args.checksums:
        sums=root/'SHA256SUMS'
        errors=[];listed=set()
        for line in sums.read_text().splitlines():
            digest,rel=line.split('  ',1);listed.add(rel);p=root/rel
            if not p.is_file() or hashlib.sha256(p.read_bytes()).hexdigest()!=digest:errors.append(rel)
        actual={str(p.relative_to(root)) for p in root.rglob('*') if p.is_file() and p.name!='SHA256SUMS' and '__pycache__' not in p.parts}
        check('SHA256 file integrity',not errors,', '.join(errors))
        check('SHA256 inventory complete',listed==actual,', '.join(sorted(listed^actual)))
    result={'artifact':'Design specification only','checks':checks,'passed':sum(x['result']=='PASS' for x in checks),'failed':sum(x['result']=='FAIL' for x in checks),
            'counts':{n:len(x) for n,x in datasets.items()},'dotnetBuild':'NOT_EXECUTED','sqlRuntimeTests':'NOT_EXECUTED','liveOwnerE2E':'NOT_EXECUTED','productionCertification':False}
    encoded=json.dumps(result,indent=2)
    if args.output:Path(args.output).write_text(encoded+'\n')
    print(encoded)
    return 1 if result['failed'] else 0

if __name__=='__main__':
    raise SystemExit(main())
