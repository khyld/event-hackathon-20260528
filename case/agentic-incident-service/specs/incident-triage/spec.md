# Feature: Incident details + recommendation

## Goal
Enable users to open an incident, view its details, and receive a deterministic recommendation for the next best action.

## Scope
- Expose incident detail retrieval for a single incident.
- Expose recommendation generation for a single incident.
- Reuse synthetic incident data from `data/incidents.json` as the single source of truth.
- Keep the API response shape stable and aligned with the existing frontend contract in `src/WebApp/src/api.ts`.

## Non-goals
- Building a real AI model or external classifier.
- Persisting incidents beyond the in-memory data loaded from the JSON file.
- Adding authentication, authorization, or role-based access control.
- Implementing advanced triage workflows, escalation rules, or multi-incident analytics.

## Data source
- The authoritative dataset is `data/incidents.json`.
- Each record must contain all fields the frontend expects: `id`, `title`, `severity`, `system`, `tags`, `observedAt`, `category`, `priority`, `description`, `reason`.
- The `source` field is metadata; it is not returned to the frontend.
- If a record is missing an optional field (`tags`, `reason`), the API returns an empty array or empty string, never `null`.

## API contracts

The response schema below must match the current frontend expectations in `src/WebApp/src/api.ts`.

### GET /api/incidents/{id}
Returns one incident by identifier.

Response body:
```json
{
  "id": "INC-001",
  "title": "Payments failing for some customers",
  "severity": "High",
  "system": "Payments",
  "tags": ["api", "gateway", "timeouts"],
  "observedAt": "2026-05-20T09:12:00Z",
  "category": "Availability",
  "priority": "P1",
  "description": "Intermittent 502 errors on /charge endpoint.",
  "reason": "Multiple customers affected by payment failures during peak hours."
}
```

### GET /api/incidents/{id}/recommendation
Returns a deterministic recommendation for the selected incident.

Response body:
```json
{
  "incidentId": "INC-001",
  "summary": "Investigate the payment gateway dependency and recent deployment changes.",
  "nextAction": "Check gateway error logs, retry metrics, and the latest deployment for the payments service.",
  "confidence": "High"
}
```

### ID format
A valid incident identifier matches the pattern `INC-\d{3}` (e.g. `INC-001`). Any string that does not match this pattern — including empty, whitespace-only, or otherwise malformed values — is invalid.

### Validation and error rules
- Invalid IDs (do not match `INC-\d{3}`) → `400 Bad Request`.
- Valid format but not found in the dataset → `404 Not Found`.
- All responses (success and error) must use `Content-Type: application/json`.
- Error responses must use this body shape:
  ```json
  { "error": "<human-readable message>", "status": 400 }
  ```
- Recommendation output must be a pure function of the incident record. It must not depend on the clock, random input, request order, or the full dataset. The same incident must produce the same recommendation text across repeated requests and app restarts.
- The API must use synthetic data only; it must not depend on external AI services, external databases, or secrets.

## Acceptance criteria
1. `GET /api/incidents/{id}` returns a JSON object with exactly these fields: `id`, `title`, `severity`, `system`, `tags`, `observedAt`, `category`, `priority`, `description`, `reason`. No extra fields are included.
2. `GET /api/incidents/{id}` returns `400` with a JSON error body for IDs that do not match `INC-\d{3}`, and `404` with a JSON error body for valid-format IDs not in the dataset.
3. `GET /api/incidents/{id}/recommendation` returns a JSON object with exactly these fields: `incidentId`, `summary`, `nextAction`, `confidence`.
4. `GET /api/incidents/{id}/recommendation` returns `400` / `404` with the same rules and error body as the detail endpoint.
5. Calling the recommendation endpoint twice for the same incident — including across an app restart — returns identical `summary`, `nextAction`, and `confidence` values.
6. All responses use `Content-Type: application/json`.
7. The API loads data from `data/incidents.json` and does not require external services, databases, or secrets.

## Edge cases
- Unknown but correctly formatted identifier (e.g. `INC-999`).
- Invalid format identifiers: empty string, whitespace, `abc`, `INC001`, `inc-001`.
- Incident record missing optional fields (`tags` absent or empty, `reason` absent).
- Recommendation requested for an incident with a missing or unrecognized severity.
- Concurrent or repeated requests for the same incident during frontend refresh.
- Trailing slashes or extra path segments after the ID.

## Test plan

### API unit tests (required)
| # | Test | Expected |
|---|---|---|
| 1 | Valid ID returns incident detail | 200, body has exactly the 10 `IncidentDetails` fields |
| 2 | Valid ID returns recommendation | 200, body has exactly the 4 `Recommendation` fields |
| 3 | Invalid format ID (`abc`) on detail | 400, JSON error body |
| 4 | Invalid format ID (`abc`) on recommendation | 400, JSON error body |
| 5 | Unknown ID (`INC-999`) on detail | 404, JSON error body |
| 6 | Unknown ID (`INC-999`) on recommendation | 404, JSON error body |
| 7 | Repeated recommendation calls return identical output | assert `summary`, `nextAction`, `confidence` are equal |
| 8 | All success responses have `Content-Type: application/json` | header check |
| 9 | All error responses have `Content-Type: application/json` | header check |
| 10 | Incident with empty `tags` returns `[]`, not null | field check |
| 11 | Incident with missing `reason` returns `""`, not null | field check |
| 12 | Detail response contains no extra fields beyond the 10 | strict schema check |
| 13 | Recommendation response contains no extra fields beyond the 4 | strict schema check |

### API determinism test (required)
| # | Test | Expected |
|---|---|---|
| 14 | Call recommendation, restart app host, call again | identical output |
| 15 | 5 concurrent recommendation calls for same incident | all responses identical |

### Integration tests (required)
| # | Test | Expected |
|---|---|---|
| 16 | Frontend loads detail + recommendation for a valid incident | both panels render |
| 17 | Frontend shows error state for unknown incident | not-found message visible |
| 18 | Frontend handles recommendation endpoint failure gracefully | detail still renders, recommendation shows error |

### Contract tests (required)
| # | Test | Expected |
|---|---|---|
| 19 | Detail response schema matches `IncidentDetails` type in `api.ts` | no missing or extra fields |
| 20 | Recommendation response schema matches `Recommendation` type in `api.ts` | no missing or extra fields |

### Manual validation
- Open an incident from the list and confirm the detail panel renders all expected fields.
- Confirm the recommendation panel updates for the selected incident.
- Confirm invalid or unknown IDs produce a clear, user-facing error.
