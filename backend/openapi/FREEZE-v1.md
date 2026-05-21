# OpenAPI v1 freeze policy

| Field | Value |
|-------|--------|
| **Spec file** | `openapi/v1.yaml` |
| **Status** | **Partial freeze** (2026-05-19) — contract tracks implementation; formal Architect sign-off pending |
| **FE codegen** | `production/frontend`: `npm run codegen:api` → `src/api/schema.d.ts` |

## Allowed without version bump

- New paths under `/api/v1/...`
- New optional request/response properties
- New enum values documented in changelog
- Descriptions, examples, tags

## Requires v2 process

- Renaming or removing paths
- Changing required fields or types on existing operations
- Changing auth scheme or breaking pagination contract
- Renaming `operationId` used by codegen clients

## Process

1. Edit `openapi/v1.yaml`.
2. Run `npm run codegen:api` in `production/frontend`.
3. Add row to `production/ops/CHANGELOG-API.md`.
4. CI: `validate-openapi-codegen` job must pass (no drift).
5. Architect reviews diff on PRs touching `openapi/`.

## Related

- IMP-2 in `production/docs/IMPLEMENT-GATE.md`
- `technical-plan.md` API section
